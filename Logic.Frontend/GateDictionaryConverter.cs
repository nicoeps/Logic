using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logic.Core.IO;
using Logic.Core.LogicGate;
using Logic.Frontend.LogicGate;
using AndGate = Logic.Frontend.LogicGate.AndGate;
using Clock = Logic.Frontend.LogicGate.Clock;
using Light = Logic.Frontend.LogicGate.Light;
using NandGate = Logic.Frontend.LogicGate.NandGate;
using NorGate = Logic.Frontend.LogicGate.NorGate;
using NotGate = Logic.Frontend.LogicGate.NotGate;
using OrGate = Logic.Frontend.LogicGate.OrGate;
using Toggle = Logic.Frontend.LogicGate.Toggle;
using XnorGate = Logic.Frontend.LogicGate.XnorGate;
using XorGate = Logic.Frontend.LogicGate.XorGate;

namespace Logic.Frontend
{
    public class GateDictionaryConverter : JsonConverter<Dictionary<int, IDraw>>
    {
        public override Dictionary<int, IDraw> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray || !reader.Read())
            {
                throw new JsonException();
            }

            Dictionary<int, IDraw> gates = new Dictionary<int, IDraw>();
            Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                string type = "";
                int id = -1;
                Vector2 pos = Vector2.Zero;
                int currentInputs = 0;
                List<int> inputs = new List<int>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        IDraw gate = type switch
                        {
                            "NOT" => new NotGate(id, pos),
                            "AND" => new AndGate(id, pos),
                            "NAND" => new NandGate(id, pos),
                            "OR" => new OrGate(id, pos),
                            "NOR" => new NorGate(id, pos),
                            "XOR" => new XorGate(id, pos),
                            "XNOR" => new XnorGate(id, pos),
                            "Light" => new Light(id, pos),
                            "Toggle" => new Toggle(id, pos),
                            "Clock" => new Clock(id, pos),
                            _ => throw new JsonException()
                        };
                        int change = currentInputs - ((Gate) gate).currentInputs;
                        for (int i = 0; i < change; ++i)
                        {
                            ((Gate) gate).IncreaseInputs();
                        }
                        gates.Add(id, gate);
                        connections.Add(id, inputs);
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();
                        switch (propertyName)
                        {
                            case "type":
                                type = reader.GetString();
                                break;
                            case "id":
                                id = reader.GetInt32();
                                break;
                            case "pos":
                                reader.Read();
                                pos.X = reader.GetInt32();
                                reader.Read();
                                pos.Y = reader.GetInt32();
                                break;
                            case "currentInputs":
                                currentInputs = reader.GetInt32();
                                break;
                            case "inputs":
                                inputs.Clear();
                                reader.Read();
                                while (reader.TokenType != JsonTokenType.EndArray)
                                {
                                    inputs.Add(reader.GetInt32());
                                    reader.Read();
                                }
                                break;
                        }
                    }
                }
            }

            foreach (KeyValuePair<int,List<int>> connection in connections)
            {
                if (gates.TryGetValue(connection.Key, out IDraw gate))
                {
                    for (int i = 0; i < connection.Value.Count; ++i)
                    {
                        if (gates.TryGetValue(connection.Value[i], out IDraw input))
                        {
                            ((Gate) gate).SetInput(i, (Gate) input);
                        }
                    }
                }
            }

            return gates;
        }


        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<int, IDraw> gates,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (IDraw gate in gates.Values)
            {
                writer.WriteStartObject();
                string type = gate switch
                {
                    NotGate _ => "NOT",
                    AndGate _ => "AND",
                    NandGate _ => "NAND",
                    OrGate _ => "OR",
                    NorGate _ => "NOR",
                    XorGate _ => "XOR",
                    XnorGate _ => "XNOR",
                    Light _ => "Light",
                    Toggle _ => "Toggle",
                    Clock _ => "Clock",
                    _ => "Error"
                };
                writer.WriteString("type", type);
                writer.WriteNumber("id", gate.ID);
                writer.WriteStartArray("pos");
                writer.WriteNumberValue(gate.Pos.X);
                writer.WriteNumberValue(gate.Pos.Y);
                writer.WriteEndArray();
                writer.WriteNumber("currentInputs", ((Gate) gate).currentInputs);
                writer.WriteStartArray("inputs");
                foreach (Input input in ((Gate) gate).inputs)
                {
                    writer.WriteNumberValue(((IDraw) input.output)?.ID ?? -1);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
