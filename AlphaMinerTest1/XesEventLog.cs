using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlphaMinerTest1
{
    public static class XesEventLog
    {
        public static EventLog Parse(XDocument document)
        {
            var traces = document.Root
                .Elements("trace")
                .Select(traceElement => traceElement
                    .Elements("event")
                    .Select(eventElement => new Event(
                        name: eventElement.GetXesAttribute("concept:name"),
                        transition: eventElement.GetXesAttribute("lifecycle:transition"),
                        timestamp: Convert.ToDateTime(eventElement.GetXesAttribute("time:timestamp")),
                        resource: eventElement.GetXesAttribute("org:resource"))));

            return new EventLog(traces.Select(trace => new Trace(trace.ToArray())).ToArray());
        }

        private static string GetXesAttribute(this XElement element, string key)
        {
            return element.Elements().First(e => e.Attribute("key").Value == key).Attribute("value").Value;
        }
    }
}