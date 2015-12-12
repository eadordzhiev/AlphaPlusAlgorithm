using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AlphaMinerTest1
{
    static class Program
    {
        static void Main(string[] args)
        {
            var file = File.OpenText("repairExampleCleansed.xes");
            var document = XDocument.Load(file);

            var log = XesEventLog.Parse(document);

            //// Loop of length 1
            //var log = ParseLog("adddb,addccb,adcdcb,accb,ab");

            //// Loop of length 2
            //var log = ParseLog("abcbcbcbd,abd");

            //// Loop of length 1 and length 2
            //var log = ParseLog("abbbcdcdbbceedbbcf,abcdbcedcdcef,acdcdcf,acdcdceef,acf");

            var net = AlphaMiner.MinePetriNet(log);

            var pnml = ExportToPNML(net).ToString();
            
        }

        static EventLog ParseLog(string log)
        {
            return new EventLog(log.Split(',').Select(trace => new Trace(trace.Select(c => new Event(c.ToString())).ToArray())).ToArray());
        }

        static XElement ExportToPNML(PetriNet net)
        {
            var page = new XElement("page", new XAttribute("id", "page1"));
            page.Add(net.Places.Select(place => new XElement("place", new XAttribute("id", place))));
            page.Add(net.Transitions.Select(transition => new XElement("transition", new XAttribute("id", transition))));

            int counter = 0;
            foreach (var arc in net.Arcs)
            {
                page.Add(
                    new XElement("arc",
                        new XAttribute("id", "arc" + counter),
                        new XAttribute("source", arc.Source),
                        new XAttribute("target", arc.Target)));
                counter++;
            }

            return
                new XElement("pnml",
                    new XElement("net",
                        new XAttribute("id", "net1"),
                        new XAttribute("type", @"http://www.pnml.org/version-2009/grammar/pnmlcoremodel"),
                        page));
        }
    }
}
