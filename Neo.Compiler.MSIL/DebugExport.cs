using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vmtool;

namespace Neo.Compiler
{
    static class DebugExport
    {
        static MyJson.JsonNode_Array GetSequencePoints(NeoModule module)
        {
            return GetSequencePoints(module.total_Codes.Values);
        }

        private static MyJson.JsonNode_Array GetSequencePoints(IEnumerable<NeoCode> codes)
        {
            var points = codes
                .Where(code => code.sequencePoint != null)
                .Select(code => (code.addr, code.sequencePoint));

            var outjson = new MyJson.JsonNode_Array();

            foreach (var (address, sequencePoint) in points)
            {
                var spjson = new MyJson.JsonNode_Object();
                spjson.SetDictValue("address", address);
                spjson.SetDictValue("document", sequencePoint.Document.Url);
                spjson.SetDictValue("start-line", sequencePoint.StartLine);
                spjson.SetDictValue("start-column", sequencePoint.StartColumn);
                spjson.SetDictValue("end-line", sequencePoint.EndLine);
                spjson.SetDictValue("end-column", sequencePoint.EndColumn);

                outjson.Add(spjson);
            }

            return outjson;
        }

        static MyJson.JsonNode_Array GetMethods(NeoModule module)
        {
            var outjson = new MyJson.JsonNode_Array();

            foreach (var method in module.mapMethods.Values)
            {
                var methodJson = new MyJson.JsonNode_Object();
                methodJson.SetDictValue("namespace", method._namespace);
                methodJson.SetDictValue("name", method.name);
                methodJson.SetDictValue("display-name", method.displayName);
                methodJson.SetDictValue("start-address", method.body_Codes.Values.First().addr);
                methodJson.SetDictValue("end-address", method.body_Codes.Values.Last().addr);

                var paramsJson = new MyJson.JsonNode_Array();
                foreach (var param in method.paramtypes)
                {
                    var paramJson = new MyJson.JsonNode_Object();
                    paramJson.SetDictValue("name", param.name);
                    paramJson.SetDictValue("type", FuncExport.ConvType(param.type));
                    paramsJson.Add(paramJson);
                }
                methodJson.SetDictValue("parameters", paramsJson);

                methodJson.SetDictValue("return-type", FuncExport.ConvType(method.returntype));

                var varaiablesJson = new MyJson.JsonNode_Array();
                foreach (var variable in method.body_Variables)
                {
                    var variableJson = new MyJson.JsonNode_Object();
                    variableJson.SetDictValue("name", variable.name);
                    variableJson.SetDictValue("type", FuncExport.ConvType(variable.type));
                    varaiablesJson.Add(variableJson);
                }
                methodJson.SetDictValue("variables", varaiablesJson);

                methodJson.SetDictValue("sequence-points", GetSequencePoints(method.body_Codes.Values));
                outjson.Add(methodJson);
            }
            return outjson;
        }

        public static MyJson.JsonNode_Object Export(NeoModule am)
        {
            var outjson = new MyJson.JsonNode_Object();
            outjson.SetDictValue("entrypoint", am.mainMethod);
            outjson.SetDictValue("methods", GetMethods(am));
            outjson.SetDictValue("sequence-points", GetSequencePoints(am));
            return outjson;
        }
    }
}
