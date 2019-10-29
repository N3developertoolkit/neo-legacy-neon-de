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

        static string ConvertType(string type)
        {
            if (type == "System.Object")
                return string.Empty;

            return FuncExport.ConvType(type);
        }

        static MyJson.JsonNode_Array GetParameters(IList<NeoParam> @params)
        {
            var paramsJson = new MyJson.JsonNode_Array();
            foreach (var param in @params)
            {
                var paramJson = new MyJson.JsonNode_Object();
                paramJson.SetDictValue("name", param.name);
                paramJson.SetDictValue("type", ConvertType(param.type));
                paramsJson.Add(paramJson);
            }

            return paramsJson;
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
                methodJson.SetDictValue("parameters", GetParameters(method.paramtypes));
                methodJson.SetDictValue("return-type", ConvertType(method.returntype));

                var varaiablesJson = new MyJson.JsonNode_Array();
                foreach (var variable in method.body_Variables)
                {
                    var variableJson = new MyJson.JsonNode_Object();
                    variableJson.SetDictValue("name", variable.name);
                    variableJson.SetDictValue("type", ConvertType(variable.type));
                    varaiablesJson.Add(variableJson);
                }
                methodJson.SetDictValue("variables", varaiablesJson);

                methodJson.SetDictValue("sequence-points", GetSequencePoints(method.body_Codes.Values));
                outjson.Add(methodJson);
            }
            return outjson;
        }

        static MyJson.JsonNode_Array GetEvents(NeoModule module)
        {
            var outjson = new MyJson.JsonNode_Array();
            foreach (var @event in module.mapEvents.Values)
            {
                var eventJson = new MyJson.JsonNode_Object();
                eventJson.SetDictValue("namespace", @event._namespace);
                eventJson.SetDictValue("name", @event.name);
                eventJson.SetDictValue("display-name", @event.displayName);
                eventJson.SetDictValue("parameters", GetParameters(@event.paramtypes));
                eventJson.SetDictValue("return-type", ConvertType(@event.returntype));
                outjson.Add(eventJson);
            }
            return outjson;
        }

        public static MyJson.JsonNode_Object Export(NeoModule am)
        {
            var outjson = new MyJson.JsonNode_Object();
            outjson.SetDictValue("entrypoint", am.mainMethod);
            outjson.SetDictValue("methods", GetMethods(am));
            outjson.SetDictValue("events", GetEvents(am));
            outjson.SetDictValue("sequence-points", GetSequencePoints(am));
            return outjson;
        }
    }
}
