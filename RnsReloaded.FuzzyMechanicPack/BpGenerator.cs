using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FuzzyMechanicPack {
    internal unsafe class BpGenerator {
        IRNSReloaded rnsReloaded;
        IUtil utils;

        public BpGenerator(IRNSReloaded rnsReloaded) {
            this.rnsReloaded = rnsReloaded;
            this.utils = rnsReloaded.utils;
        }

        protected void execute_pattern(CInstance* self, CInstance* other, string pattern, RValue[] args) {
            this.rnsReloaded.ExecuteScript("bpatt_var", self, other, args);
            args = [new RValue(this.rnsReloaded.ScriptFindId(pattern))];
            this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);
            this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
        }

        protected RValue[] add_if_not_null(RValue[] args, string fieldName, int? value) {
            if (value != null) {
                return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
            }
            return args;
        }

        protected RValue[] add_if_not_null(RValue[] args, string fieldName, bool? value) {
            if (value != null) {
                return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
            }
            return args;
        }

        protected RValue[] add_if_not_null(RValue[] args, string fieldName, double? value) {
            if (value != null) {
                return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
            }
            return args;
        }
    }
}
