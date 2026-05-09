import idc
import idaapi
import idautils
from collections import defaultdict

def find_strings_contain_word(word):
    if word is None:
        return None
    candidate_list=[]
    for string in idautils.Strings():
        if word in str(string):
            candidate_list.append(string)
    return candidate_list

def most_common_word(words,top_n=3):
    word_counts = {}
    for word in words:
        word = word.strip()
        if word in ["get","set","is","create","destory","add","remove"]:
            continue
        if word in word_counts:
            word_counts[word] += 1
        else:
            word_counts[word] = 1
    max_count = -1
    most_common_word = None
    
    sorted_words = sorted(word_counts.items(), key=lambda x: x[1], reverse=True)
    top_n_words = [word for word, count in sorted_words[:top_n]]
    return "_".join(top_n_words)

def process():
    result = find_strings_contain_word("Code_Function.cpp")
    if len(result)>1:
        print("Error! There are some similar strings here, please clarify them and modify the script code, to ensure that we have only one address, who is used by a function, who manages function names");
        print("--- candidate strings:")
        for s in result:
            print(str(s))
        print("---")
        return None
    elif len(result)==0:
        print("Error!Cannot find by string :( I do not know how to parse it...")
        return None
    str_addr = result[0].ea
    func_refs = list(idautils.DataRefsTo(str_addr))
    if len(func_refs)>1:
        print("Error! This string reference is used by many functions... I am not sure which one is correct... address > "+str(func_refs))
        return None
    elif len(func_refs)==0:
        print("Error! This string is not referenced... It should have one unique reference! address > "+str(func_refs))
        return None
    func = idaapi.get_func(func_refs[0])
    if not func:
        print("Error! This address is not a function! str addr = %s, use str ref addr = %s"%(str(str_addr),str(func_refs[0])))
        return None
        
    func_start_addr = func.start_ea
    old_func_name = idc.get_func_name(func_start_addr)
    new_name = "store_function_name_information"
    idc.set_name(func_start_addr,new_name, idc.SN_NOWARN)
    print("Rename [%s] -> [%s] @Address %s"%(old_func_name,new_name,str(func_start_addr)))
    
    func_xrefs = list(XrefsTo(func_start_addr))
    func_xrefs_call = []
    for xref in func_xrefs:
        if xref.type == idaapi.fl_CN:
            func_xrefs_call.append(xref)
    
    fail_count=0
    init_func_dict = {}
    # process each ref!
    for xref in func_xrefs_call:
        call_command_addr = xref.frm
        func_start = idaapi.get_func(call_command_addr).start_ea
        key_words=None
        if func_start in init_func_dict:
            key_words = init_func_dict[func_start]
        else:
            init_func_dict[func_start]=[]
            key_words = init_func_dict[func_start]
        print("process 0x%x"%(call_command_addr))
        cur_addr = call_command_addr
        found_string_address = None
        found_function_address = None
        # for each ref, it should look like:
        # ------
        # lea ?, func_addr
        # lea ?, string_addr
        # lea ?, ?
        # call store_function_name_information ;> the reference address is here!
        # ------
        # we search forward to get function & string address
        for trials in range(40):
            insn = ida_ua.insn_t()
            cur_addr = ida_ua.decode_prev_insn(insn, cur_addr)
            mnem = insn.get_canon_mnem()
            if mnem == "call":
                break
            if mnem !="lea":
                continue
            op_type = insn.ops[1].type
            
            if op_type == ida_ua.o_mem:
                op_addr = insn.ops[1].addr
                if found_function_address is None:
                    func = idaapi.get_func(op_addr)
                    if func:
                        found_function_address = func
                        if found_string_address is None:
                            continue
                        else:
                            break
                if found_string_address is None:
                    str_content = idc.get_strlit_contents(op_addr)
                    if str_content:
                        found_string_address = op_addr
                        if found_function_address is None:
                            continue
                        else:
                            break
            if cur_addr<=func_start:
                break
                
        if found_string_address is None or found_function_address is None:
            print(f"Failed to find functions and strings...")
            continue
        string_content = idc.get_strlit_contents(found_string_address).decode('utf-8')
        key_words+= string_content.split("_")
        func_addr = found_function_address.start_ea
        old_func_name = str(idc.get_func_name(func_addr))
        print("0x%x | Found Str @0x%x > %s | Found Func 0x%x > Old Name > %s"%(call_command_addr,found_string_address,string_content,int(func_addr),old_func_name))
        if not idc.set_name(func_addr,string_content):
            if not idc.set_name(func_addr,"gamemaker_"+string_content):
                print("Failed to rename to "+string_content)
                fail_count+=1
    print("-------")
    for k,v in init_func_dict.items():
        word = most_common_word(v)
        new_name = "gamemaker_init_func_name_%s_%d"%(word,len(v))
        print("Rename Func 0x%x > %s"%(k,new_name))
        idc.set_name(k,new_name)
    print("Total Init Func Renames %d"%(len(init_func_dict)))
    print("Total Func Renames %d, failure: %d"%(len(func_xrefs_call),fail_count))
    
if __name__ == "__main__":
    process()
            
            
    

    