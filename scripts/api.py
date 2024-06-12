from enum import Enum

try:
    from typing import List
except ImportError:
    # It's... just typing. Doesn't matter.
    pass


class OperandType(Enum):
    void = 0
    reg = 1
    mem = 2
    phrase = 3
    displ = 4
    imm = 5
    far = 6
    near = 7

class Operand:

    def __init__(self, op_type, register, address, value):
        # type: (int, str, int, int) -> None
        self.op_type = OperandType(op_type)
        self.register = register
        self.address = address
        self.value = value

    def __repr__(self):
        if self.op_type == OperandType.void: return None
        if self.op_type == OperandType.reg: return f"{self.register}"
        if self.op_type == OperandType.mem: return f"[{self.address:X}]"
        if self.op_type == OperandType.phrase: return f"[{self.register}{f'+0x{self.address:X}' if self.address != 0 else ''}{f'+{self.value:X}' if self.value != 0 else ''}]"
        if self.op_type == OperandType.displ: return f"[{self.register}{f'+0x{self.address:X}' if self.address != 0 else ''}{f'+{self.value:X}' if self.value != 0 else ''}]"
        if self.op_type == OperandType.imm: return f"0x{self.value:X}"
        # if self.op_type == OperandType.o_far: return f""
        if self.op_type == OperandType.near: return f"near 0x{self.address:X}"
        return f"{self.op_type} [{self.register}{f'+0x{self.address:X}' if self.address != 0 else ''}{f'+{self.value:X}' if self.value != 0 else ''}]"

class Instruction:
    def __init__(self, address, opcode, operands):
        # type: (int, str, List[Operand]) -> None
        self.address = address
        self.opcode = opcode
        self.operands = operands

    def __repr__(self):
        return f"{self.opcode} {', '.join([str(op) for op in self.operands if op.op_type != OperandType.void])}"

class Function:
    def __init__(self, address):
        # type (int) -> None
        self.address = address

    def __eq__(self, other):
        return type(self) == type(other) and self.address == other.address

    def __repr__(self):
        return f"<Function {self.address:X} >"

class Xref:
    def __init__(self, frm, to):
        # type: (int, int) -> None
        self.frm = frm
        self.to = to

    def __repr__(self):
        return f"<Xref {self.frm:X} -> {self.to:X} >"

class Segment:
    def __init__(self, name, start, end):
        self.name = name
        self.start = start
        self.end = end

    def __repr__(self):
        return f"<Segment {self.name}: {self.start} ... {self.end} >"

class BaseApi(object):

    def scan(self, signature):
        # type: (str) -> int
        pass

    def get_segment(self, address):
        # type: (int) -> Segment
        pass

    def get_instructions(self, address):
        # type: (int) -> List[Instruction]
        pass

    def get_function(self, address):
        # type (int) -> Function
        pass

    def get_name(self, address):
        # type: (int) -> str
        pass

    def set_name(self, address, name, type):
        # type: (int, str, str) -> bool
        pass

    def set_type(self, address, type, ptr):
        # type: (int, str, int) -> bool
        pass

    def read_u32(self, address):
        # type: (int) -> int
        pass

    def read_u64(self, address):
        # type: (int) -> int
        pass

    def read_str(self, address):
        # type: (int) -> str
        pass

    def set_func_arg_type(self, address, index, type, ptr, name):
        # type: (int, int, str, int, str) -> bool
        pass

    def set_func_ret_type(self, address, type, ptr):
        # type: (int, str, int) -> bool
        pass

    def get_xrefs_from(self, address, verbose=False):
        #type: (int, bool) -> List[Xref]
        pass

    def get_xrefs_to(self, address, verbose=False):
        #type: (int, bool) -> List[Xref]
        pass

api = None  # type: BaseApi

try:
    import ida_auto
    import ida_bytes
    import ida_ida
    import ida_idaapi
    import ida_idp
    import ida_funcs
    import idautils
    import ida_ua
    import ida_name
    import ida_typeinf
    import ida_nalt
    import idc

    class IDAApi(BaseApi):
        def scan(self, signature):
            patterns = ida_bytes.compiled_binpat_vec_t()
            ida_bytes.parse_binpat_str(patterns, 0, signature, 16)

            addr = ida_bytes.bin_search(
                ida_ida.inf_get_min_ea(), ida_ida.inf_get_max_ea(), patterns, 0
            )
            if addr == ida_idaapi.BADADDR:
                return None

            if signature.startswith("E8") or signature.startswith("E9"):
                # Displacement
                offset = self.read_u32(addr + 1)
                if offset > 0x7FFFFFFF:
                    # Convert to signed
                    offset = ~0x7FFFFFFF | offset
                addr += 5 + offset

            ida_auto.show_addr(addr)
            return addr

        def get_segment(self, address):
            ida_auto.show_addr(address)
            return Segment(idc.get_segm_name(address), 0, 0)

        def get_instructions(self, address):
            ida_auto.show_addr(address)
            reg_names = ida_idp.ph_get_regnames()
            func = ida_funcs.get_func(address)
            if func is not None:
                insns = list(idautils.FuncItems(func.start_ea))
                for insn_addr in insns:
                    insn = ida_ua.insn_t()
                    ida_ua.decode_insn(insn, insn_addr)
                    # print(f"[{insn_addr:X}] {insn.get_canon_mnem()}") # for when
                    yield Instruction(
                        insn_addr,
                        insn.get_canon_mnem(),
                        list(Operand(op.type, reg_names[op.reg], op.addr, op.value) for op in insn.ops)
                    )

        def get_function(self, address):
            ida_auto.show_addr(address)
            func = ida_funcs.get_func(address)
            if func is None:
                return None
            return Function(func.start_ea)

        def get_name(self, address):
            ida_auto.show_addr(address)
            func_name = ida_funcs.get_func_name(address)
            if func_name is None:
                return ida_name.get_name(address)
            return func_name

        def set_name(self, address, name, type):
            ida_auto.show_addr(address)
            return ida_name.set_name(address, name, ida_name.SN_FORCE)

        def set_type(self, address, type, ptr):
            ida_auto.show_addr(address)
            type_tinfo = self.create_type_object(type, ptr)
            if type_tinfo is None:
                print(f"[{address:X}]: Could not create type object")
                return False

            return ida_typeinf.apply_tinfo(address, type_tinfo, ida_typeinf.TINFO_DEFINITE)

        def read_u32(self, address):
            ida_auto.show_addr(address)
            return ida_bytes.get_wide_dword(address)

        def read_u64(self, address):
            ida_auto.show_addr(address)
            return ida_bytes.get_qword(address)

        def read_str(self, address):
            ida_auto.show_addr(address)
            str = ida_bytes.get_strlit_contents(address, -1, ida_nalt.STRTYPE_C)
            if str is None:
                return None
            return str.decode("utf-8")

        def create_primitive_type(self, tinfo_t, type):
            primitives = {
                'void': ida_typeinf.BTF_VOID,
                'char': ida_typeinf.BTF_CHAR,
                'int': ida_typeinf.BTF_INT,
                'float': ida_typeinf.BTF_FLOAT,
                'double': ida_typeinf.BTF_DOUBLE,
                'bool': ida_typeinf.BTF_BOOL,
                'int8': ida_typeinf.BTF_INT8,
                'int16': ida_typeinf.BTF_INT16,
                'int32': ida_typeinf.BTF_INT32,
                'int64': ida_typeinf.BTF_INT64,
                'uint8': ida_typeinf.BTF_UINT8,
                'uint16': ida_typeinf.BTF_UINT16,
                'uint32': ida_typeinf.BTF_UINT32,
                'uint64': ida_typeinf.BTF_UINT64,
            }
            if type in primitives:
                return tinfo_t.create_simple_type(primitives[type])
            return False

        def create_type_object(self, type, ptr):
            type_tinfo = ida_typeinf.tinfo_t()
            idati = ida_typeinf.get_idati()
            if not type_tinfo.get_named_type(idati, type):
                if not self.create_primitive_type(type_tinfo, type):
                    terminated = type + f";"
                    if not ida_typeinf.parse_decl(
                        type_tinfo, idati, terminated, ida_typeinf.PT_SIL | ida_typeinf.PT_NDC | ida_typeinf.PT_TYP
                    ):
                        print(f": Could not parse type '{terminated}'")
                        return None

            if ptr is not None:
                for _ in range(ptr):
                    new_tinfo = ida_typeinf.tinfo_t()
                    new_tinfo.create_ptr(type_tinfo)
                    type_tinfo = new_tinfo
            return type_tinfo

        def set_func_arg_type(self, address, index, type, ptr, name):
            ida_auto.show_addr(address)
            tinfo = ida_typeinf.tinfo_t()
            if not ida_nalt.get_tinfo(tinfo, address):
                print(f"[{address:X}]: Could not get adress type info")
                return False

            func_data = ida_typeinf.func_type_data_t()
            if not tinfo.get_func_details(func_data):
                print(f"[{address:X}]: Could not create function type")
                return False

            if index >= func_data.size():
                # print(f"[{address:X}]: Argument index out of range")
                # return False
                func_data.resize(index+1)

            if name is not None:
                func_data[index].name = name

            type_tinfo = self.create_type_object(type, ptr)
            if type_tinfo is None:
                print(f"[{address:X}]: Could not create type object")
                return False

            # orig_type = func_data[index].type
            # if orig_type.get_size() < type_tinfo.get_size():
            #     print(f"[{address:X}]: Argument size mismatch")
            #     return False

            func_data[index].type = type_tinfo

            new_tinfo = ida_typeinf.tinfo_t()
            if not new_tinfo.create_func(func_data):
                print(f"[{address:X}]: Could not create function")
                return False

            return ida_typeinf.apply_tinfo(address, new_tinfo, ida_typeinf.TINFO_DEFINITE)

        def set_func_ret_type(self, address, type, ptr):
            ida_auto.show_addr(address)
            func_tinfo = ida_typeinf.tinfo_t()
            if not ida_nalt.get_tinfo(func_tinfo, address):
                print(f"[{address:X}]: Could not get adress type info")
                return False

            func_data = ida_typeinf.func_type_data_t()
            if not func_tinfo.get_func_details(func_data):
                print(f"[{address:X}]: Could not create function type")
                return False

            type_tinfo = self.create_type_object(type, ptr)
            if type_tinfo is None:
                print(f"[{address:X}]: Could not create type object")
                return False

            # if func_data.rettype.get_size() < type_tinfo.get_size():
            #     if(type_tinfo.get_size() != 0xffffffffffffffff): # void type returns this size
            #         print(f"[{address:X}]: Argument size mismatch")
            #         return False

            func_data.rettype = type_tinfo
            new_tinfo = ida_typeinf.tinfo_t()
            if not new_tinfo.create_func(func_data):
                print(f"[{address:X}]: Could not create function")
                return False

            return ida_typeinf.apply_tinfo(address, new_tinfo, ida_typeinf.TINFO_DEFINITE)

        def get_xrefs_from(self, address, verbose=False):
            ida_auto.show_addr(address)
            for xref in idautils.XrefsFrom(address):
                x = Xref(xref.frm, xref.to)
                if verbose:
                    print(f"get_xrefs_to: [{address}] {x}")
                yield x

        def get_xrefs_to(self, address, verbose=False):
            ida_auto.show_addr(address)
            for xref in idautils.XrefsTo(address):
                x = Xref(xref.frm, xref.to)
                if verbose:
                    print(f"get_xrefs_to: [{address}] {x}")
                yield x

    api = IDAApi()
except ImportError:
    import ghidra
    from ghidra.program.model.address import Address

    from __main__ import *  # What the hell Ghidra

    # Create a fake typing to appease Pylance *and* Python 2.7
    class FakeTyping(object):
        def __getattr__(self, name):
            return None

    typing = None
    try:
        import typing
    except ImportError:
        typing = FakeTyping()

    # https://github.com/VDOO-Connected-Trust/ghidra-pyi-generator
    if typing.TYPE_CHECKING:
        from ghidra.ghidra_builtins import *

    # todo lmao
    class GhidraApi(BaseApi):
        def scan(self, signature):
            # Turn byte into \x00 and ?? into .
            str = ""
            chunks = signature.split(" ")
            for chunk in chunks:
                if chunk == "?" or chunk == "??":
                    str += "."
                else:
                    str += "\\x" + chunk

            results = findBytes(None, str, 1)
            if len(results) == 0:
                return None

            addr = results[0].getOffset()

            if signature.startswith("E8") or signature.startswith("E9"):
                offset = self.read_u32(addr + 1)
                offset = ~0x7FFFFFFF | offset  # convert to signed
                addr += 5 + offset

            return addr

        def get_instructions(self, address):
            listing = currentProgram.getListing()
            address_factory = currentProgram.getAddressFactory()
            address_space = address_factory.getDefaultAddressSpace()
            codeUnit = listing.getCodeUnitAt(address_space.getAddress(address))
            while codeUnit is not None:
                if codeUnit.getMnemonicString() == "RET":
                    break

                operands = []
                for i in range(codeUnit.getNumOperands()):
                    for o in codeUnit.getOpObjects(i):
                        if isinstance(o, ghidra.program.model.address.Address):
                            operands.append(o.getOffset())
                        elif isinstance(o, ghidra.program.model.scalar.Scalar):
                            operands.append(o.getUnsignedValue())
                        elif isinstance(o, ghidra.program.model.symbol.Reference):
                            operands.append(o.getToAddress().getOffset())

                yield Instruction(
                    codeUnit.getMinAddress().getOffset(),
                    codeUnit.getMnemonicString().lower(),
                    # idk how this api works lol
                    operands,
                    operands,
                )

                codeUnit = listing.getInstructionAfter(codeUnit.getAddress())

        def set_name(self, address, name, type):
            symbol_table = currentProgram.getSymbolTable()
            address_factory = currentProgram.getAddressFactory()
            address_space = address_factory.getDefaultAddressSpace()
            addr = address_space.getAddress(address)

            # Ghidra is bad and some of these are UndefinedFunctions
            function = getFunctionAt(addr)
            if function is not None:
                function.setName(
                    name, ghidra.program.model.symbol.SourceType.USER_DEFINED
                )
            elif type == "function" and function is None:
                # Make the function for Ghidra since it's bad
                function = createFunction(addr, name)
            else:
                symbol_table.createLabel(
                    addr, name, ghidra.program.model.symbol.SourceType.USER_DEFINED
                )

            return True

        def read_str(self, address):
            address_factory = currentProgram.getAddressFactory()
            address_space = address_factory.getDefaultAddressSpace()
            addr = address_space.getAddress(address)
            str = ""
            while True:
                byte = getByte(addr)
                if byte == 0:
                    break
                str += chr(byte)
                addr = addr.add(1)

            return str

        def read_bytes(self, address, size):
            address_factory = currentProgram.getAddressFactory()
            address_space = address_factory.getDefaultAddressSpace()
            addr = address_space.getAddress(address)
            bytes = getBytes(addr, size)

            # Convert to unsigned
            new_bytes = []
            for i in range(size):
                if bytes[i] < 0:
                    new_bytes.append(bytes[i] + 256)
                else:
                    new_bytes.append(bytes[i])

            return new_bytes

        def read_u32(self, address):
            bytes = self.read_bytes(address, 4)
            return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24)

        def read_u64(self, address):
            one = self.read_u32(address)
            two = self.read_u32(address + 4)
            return one | (two << 32)

        # TODO type args

    api = GhidraApi()
