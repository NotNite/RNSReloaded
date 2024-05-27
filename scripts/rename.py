import sys
import os

# Clear import cache
if "api" in sys.modules:
    del sys.modules["api"]

sys.path.append(os.path.dirname(os.path.realpath(__file__)))
__package__ = "api"  # is this correct?
from api import api, OperandType, Operand, Instruction


def handle_vars(addr):
    global api
    print("handle_vars: {:02X}".format(addr))
    while True:
        variable = api.read_u64(addr)
        if variable == 0:
            break

        string = api.read_str(api.read_u64(variable))
        api.set_name(variable + 8, "var_" + string, None)

        addr += 8


def handle_funcs(addr):
    global api
    print("handle_funcs: {:02X}".format(addr))
    while True:
        func_addr = api.read_u64(addr)
        if func_addr == 0:
            break

        func_name_addr = api.read_u64(func_addr)
        if func_name_addr == 0:
            break

        func_name = api.read_str(func_name_addr)

        reference = func_addr + 8
        api.set_name(reference, "func_" + func_name, None)

        addr += 8


def handle_gml_funcs(addr, num_funcs):
    global api
    print("handle_gml_funcs: {:02X}".format(addr))
    i = 0
    while True:
        if i >= num_funcs:
            break
        i += 1

        func_addr = api.read_u64(addr)
        if func_addr == 0:
            break

        func_name = api.read_str(func_addr)
        if func_name is None:
            break

        func = api.read_u64(addr + 8)
        api.set_name(func, func_name, "function")

        # is this correct?
        func_ref = api.read_u64(addr + 16) + 8
        api.set_name(func_ref, "funcref_" + func_name, None)

        api.set_func_arg_type(func, 0, "CInstance", 1, "self")
        api.set_func_arg_type(func, 1, "CInstance", 1, "other")
        api.set_func_arg_type(func, 2, "RValue", 1, "return_value")
        api.set_func_arg_type(func, 3, "int", 0, "num_args")
        api.set_func_arg_type(func, 4, "RValue", 2, "args")
        api.set_func_ret_type(func, "RValue", 1)

        addr += 24


def parse_init_llvm():
    global api
    init_llvm = api.scan("E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 8B 41 14")
    if init_llvm is None:
        return
    print("init_llvm: {:02X}".format(init_llvm))

    insns = list(api.get_instructions(init_llvm))
    for insn in insns:
        if len(insn.operands) < 1:
            continue

        if insn.opcode == "mov":
            offset = insn.operands[0].address
            if offset == 0x18 or offset == 0x20 or offset == 0x28:
                for i in range(insns.index(insn) - 1, -1, -1):
                    insn2 = insns[i]
                    if len(insn2.operands) < 1:
                        continue

                    if insn2.opcode == "lea":
                        # Shitty Ghidra hack
                        addr = None
                        if len(insn2.operands) == 1:
                            addr = insn2.operands[0].address
                        else:
                            addr = insn2.operands[1].address

                        if offset == 0x18:
                            handle_vars(addr)
                        if offset == 0x20:
                            handle_funcs(addr)
                        if offset == 0x28:
                            num_funcs = 0
                            for j in range(insns.index(insn2) + 1, len(insns)):
                                if (
                                    insns[j].opcode == "mov"
                                    and insns[j].operands[0].address == 0x14
                                ):
                                    num_funcs = insns[j].operands[1].value
                                    break

                            handle_gml_funcs(addr, num_funcs)
                        break

def parse_const_strings():
    global api
    yy_const_string = api.scan("48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 8B DA B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 44 24 ??")
    if yy_const_string is None:
        return
    print("yy_const_string: {:02X}".format(yy_const_string))

    for xref in api.get_xrefs_to(yy_const_string):
        addr = xref.frm
        # print(f"xref found at {addr:X}")
        insns = list(api.get_instructions(addr))
        for (i, insn) in enumerate(insns):
            # print(insn)
            if insn.opcode == "lea" and \
                    insn.operands[0].op_type == OperandType.reg and \
                    insn.operands[0].register == "dx" and \
                    insn.operands[1].op_type == OperandType.mem:
                str_addr = insn.operands[1].address
                if insns[i+1].opcode == "lea" and \
                        insns[i+1].operands[0].op_type == OperandType.reg and \
                        insns[i+1].operands[0].register == "cx" and \
                        insns[i+1].operands[1].op_type == OperandType.mem:
                    rvalue_addr = insns[i+1].operands[1].address
                    if insns[i+2].opcode == "call" and \
                            insns[i+2].operands[0].op_type == OperandType.near and \
                            insns[i+2].operands[0].address == yy_const_string:
                        rvalue_name = api.get_name(rvalue_addr)
                        str_name = api.get_name(str_addr)
                        print(f"[{addr:X}] YYConstString({rvalue_name} [{rvalue_addr:X}], {str_name} [{str_addr:X}])")
                        api.set_name(rvalue_addr, f"static_var_str_{str_name}", None)

def sig_replace(
    sig: str, name: str, args: list[tuple[str, int, str]], ret: tuple[str, int]
):
    global api
    scan = api.scan(sig)
    if scan is None:
        print(f"Could not find {name}")
        return
    if not api.set_name(scan, name, "function"):
        print(f"[{scan:X}] {name}: Error setting function name")
    for i, arg in enumerate(args):
        if not api.set_func_arg_type(scan, i, arg[0], arg[1], arg[2]):
            print(f"[{scan:X}] {name}: Error setting argument {arg[2]}")
    if not api.set_func_ret_type(scan, ret[0], ret[1]):
        print(f"[{scan:X}] {name}: Error setting return type")


def static_renames():
    sig_replace("E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 8B 41 14",
        "InitLLVM", [("void", 1, "llvm_vars")], ("void", 0)) # ("SLLVMVars", 1, "llvm_vars")

    sig_replace("E8 ?? ?? ?? ?? EB 3A 48 8B 0D ?? ?? ?? ??",
        "InitGMLFunctions", [], ("void", 1))

    # sig_replace("", "", [("", 0, "")], ("", 0))
    sig_replace("C7 41 ?? ?? ?? ?? ?? 48 8B C1 48 C7 01 ?? ?? ?? ??",
        "RValue::RValue", [("RValue", 1, "self")], ("RValue", 1))
    sig_replace("C7 41 ?? ?? ?? ?? ?? 48 8B C1 48 89 11",
        "RValue::RValue(int64)", [("RValue", 1, "self"),("int64", 0, "value")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? 8D 4E 03",
        "RValue::RValue(int)", [("RValue", 1, "self"),("int", 0, "value")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? C7 44 24 ?? ?? ?? ?? ?? 8D 4E 01",
        "RValue::RValue(double)", [("RValue", 1, "self"),("double", 0, "value")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? 89 7D 40",
        "RValue::operator+=", [("RValue", 1, "self"),("RValue", 1, "rhs")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? 45 8B F5",
        "RValue::operator-=", [("RValue", 1, "self"),("RValue", 1, "rhs")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? 48 89 85 ?? ?? ?? ?? 83 FE 01",
        "RValue::operator*", [("RValue", 1, "result"),("RValue", 1, "lhs"),("RValue", 1, "rhs")], ("RValue", 1))
    # sig_replace("E8 ?? ?? ?? ?? 8B 7D 20",
    #     "RValue::operator*=", [("RValue", 1, "self"),("RValue", 1, "rhs")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? 48 8D 55 2C",
        "RValue::operator/=", [("RValue", 1, "self"),("RValue", 1, "rhs")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? 48 8D 55 18 48 8D 4C 24 ??",
        "RValue::operator%=", [("RValue", 1, "self"),("RValue", 1, "rhs")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? 84 C0 75 12 FF C3",
        "RValue::operator==", [("RValue", 1, "self"),("RValue", 1, "rhs")], ("bool", 0))

    sig_replace("8B 41 0C 25 ?? ?? ?? ?? C3",
        "KIND_RValue", [("RValue", 1, "rvalue")], ("enum RValueType", 0))
    sig_replace("E8 ?? ?? ?? ?? 8B 75 04",
        "BOOL_RValue", [("RValue", 1, "rvalue")], ("bool", 0))
    sig_replace("F7 41 ?? ?? ?? ?? ?? 75 05",
        "REAL_RValue", [("RValue", 1, "rvalue")], ("double", 0))
    sig_replace("E8 ?? ?? ?? ?? F2 0F 10 0B",
        "REAL_RValue_Ex", [("RValue", 1, "rvalue")], ("double", 0))
    sig_replace("E8 ?? ?? ?? ?? 29 03",
        "INT32_RValue", [("RValue", 1, "rvalue")], ("int32", 0))
    # sig_replace("E8 ?? ?? ?? ?? A8 40",
    #     "INT64_RValue", [("RValue", 1, "rvalue")], ("int64", 0))
    # sig_replace("E8 ?? ?? ?? ?? 8B 45 C7",
    #     "STRING_RValue", [("char", 2, "current"),("char", 2, "base"),("int", 1, "max_length"),("RValue", 1, "rvalue")], ("void", 0))
    sig_replace("E8 ?? ?? ?? ?? 8B 56 38",
        "SET_RValue", [("RValue", 1, "dest"), ("RValue", 1, "src"), ("YYObjectBase", 1, "self"), ("int", 0, "index")], ("void", 0))
    sig_replace("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B D9 BE ?? ?? ?? ?? 8B 49 0C 44 8B C6 83 E1 1F 48 8B FA 41 D3 E0 41 F6 C0 46 74 08 48 8B CB E8 ?? ?? ?? ?? 8B 4F 0C",
        "COPY_RValue", [("RValue", 1, "dest"), ("RValue", 1, "src")], ("void", 0))
    sig_replace("E8 ?? ?? ?? ?? 90 44 8B C6",
        "COPY_RValue__Post", [("RValue", 1, "dest"), ("RValue", 1, "src")], ("void", 0))
    sig_replace("E8 ?? ?? ?? ?? EB 0C 44 8B 67 08",
        "COPY_RValue_do__Post", [("RValue", 1, "dest"), ("RValue", 1, "src")], ("void", 0))
    sig_replace("E8 ?? ?? ?? ?? 8B 4B 64",
        "FREE_RValue__Pre", [("RValue", 1, "ptr")], ("void", 0))

    sig_replace(
        "E8 ?? ?? ?? ?? 4D 85 E4 74 08",
        "?dec@?$_RefThing@PEBD@@QEAAXXZ", # _RefThing<const char*>::dec()
        [("RefString", 1, "self")],
        ("void", 0),
    )

    sig_replace("E8 ?? ?? ?? ?? 4C 63 07 33 C9",
        "YYAlloc", [("int", 0, "size")], ("void", 1))
    # sig_replace("E8 ?? ?? ?? ?? 48 63 D5",
    #     "YYFree", [("void", 1, "ptr")], ("void", 0))
    sig_replace("E8 ?? ?? ?? ?? 48 8B 5D 00 4C 89 7B 20",
        "YYSetInstance", [("RValue", 1, "rvalue")], ("void", 0))
    sig_replace("E8 ?? ?? ?? ?? EB 58",
        "YYError", [("char", 1, "error")], ("void", 0))
    sig_replace("E8 ?? ?? ?? ?? 0F 57 ED",
        "YYGetReal", [("RValue", 1, "rvalue"),("int", 0, "index")], ("double", 0))
    sig_replace("E8 ?? ?? ?? ?? 44 8B 7D 77",
        "YYCompareVal", [("RValue", 1, "a"),("RValue", 1, "b"),("double", 0, "prec"),("bool", 0, "throw_error")], ("int", 0))
    sig_replace("48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 8B DA B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 44 24 ??",
        "YYConstString", [("RValue", 1, "rvalue"),("char", 1, "str")], ("void", 0))

    sig_replace("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? FF 50 08",
        "YYGML_min", [("RValue", 1, "result"),("int", 0, "argc"),("RValue", 2, "args")], ("RValue", 1))
    sig_replace("E8 ?? ?? ?? ?? 48 8B D8 8B 45 CC",
        "YYGML_max", [("RValue", 1, "result"),("int", 0, "argc"),("RValue", 2, "args")], ("RValue", 1))

    sig_replace("40 53 48 83 EC 20 48 8B D9 C7 41 ?? ?? ?? ?? ?? 48 8B 4C 24 ?? 33 D2 E8 ?? ?? ?? ?? E8 ?? ?? ?? ?? F2 0F 11 03 48 83 C4 20 5B C3 CC CC CC CC CC 40 53 48 83 EC 20 48 8B D9 C7 41 ?? ?? ?? ?? ?? 48 8B 4C 24 ?? 0F 57 C0",
        "F_Floor", [("RValue", 1, "result"),("CInstance", 1, "self"),("CInstance", 1, "other"),("int", 0, "argc"),("RValue", 2, "args")], ("void", 0))

    sig_replace("E8 ?? ?? ?? ?? 45 3B EF",
        "PushContextStack", [("YYObjectBase", 1, "obj")], ("void", 0))
    sig_replace("8B 05 ?? ?? ?? ?? 2B C1",
        "PopContextStack", [("int", 0, "num")], ("void", 0))
    sig_replace("8B 05 ?? ?? ?? ?? 85 C0 7E 11 FF C8",
        "GetContextStackTop", [], ("YYObjectBase", 1))

    sig_replace("E8 ?? ?? ?? ?? D1 FB",
        "MemoryManager::Alloc", [("size_t", 0, "size"), ("char", 1, "file"), ("uint64", 0, "_unk_u64"), ("bool", 0, "clear")], ("void", 1))

    sig_replace("E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 48 8B D3 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 8B 44 24 ??",
        "tryParseInt32", [("char", 1, "str"), ("int", 1, "result")], ("bool", 0))

    sig_replace("E8 ?? ?? ?? ?? 90 48 89 1F",
        "__ehvec_ctor", [("void", 1, "ptr"), ("size_t", 0, "size"), ("size_t", 0, "count"), ("void(*)(void*)", 0, "constructor"), ("void(*)(void*)", 0, "destructor")], ("void", 0))
    sig_replace("E8 ?? ?? ?? ?? 48 69 17 ?? ?? ?? ??",
        "__ehvec_dtor", [("void", 1, "ptr"), ("size_t", 0, "size"), ("size_t", 0, "count"), ("void (__stdcall *)(void *)", 0, "destructor")], ("void", 0))

    sig_replace("E8 ?? ?? ?? ?? 0F B6 45 09",
        "YYGML_CallScriptFunction", [("CInstance", 1, "self"),("CInstance", 1, "other"),("RValue", 1, "result"),("int",0,"num_args"),("int",0,"id"),("RValue",2,"args")], ("RValue", 1))

    sig_replace("E8 ?? ?? ?? ?? 44 8D 46 59",
        "CInstance::GetYYVarRef", [("CInstance", 1, "self"),("int", 0, "index")], ("RValue", 1))

    # sig_replace("", "", [("", 0, "")], ("", 0))
    # sig_replace("", "", [("", 0, "")], ("", 0))
    # sig_replace("", "", [("", 0, "")], ("", 0))
    # sig_replace("", "", [("", 0, "")], ("", 0))
    # sig_replace("", "", [("", 0, "")], ("", 0))
    print("---")

print("------------------------------")
static_renames()
parse_init_llvm()
parse_const_strings()
print("-------------Done-------------")