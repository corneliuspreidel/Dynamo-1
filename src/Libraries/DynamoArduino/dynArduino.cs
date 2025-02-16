﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.IO.Ports;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Models;


namespace Dynamo.Nodes
{
    [NodeName("Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Manages connection to an Arduino microcontroller.")]
    [IsVisibleInDynamoLibrary(false)]
    public partial class Arduino : NodeModel
    {
        SerialPort port;
        MenuItem comItem;

        public Arduino()
        {
            InPortData.Add(new PortData("exec", "Execution Interval"));
            OutPortData.Add(new PortData("arduino", "Serial port for later read/write"));

            RegisterAllPorts();

            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();
            }
            port = new SerialPort { BaudRate = 9600, NewLine = "\r\n", DtrEnable = true };
        }

        MenuItem lastComItem;
        
        void comItem_Checked(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)e.Source;

            if (lastComItem != null)
            {
                lastComItem.IsChecked = false; // uncheck last checked item
            }

            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();

                port.PortName = item.Header.ToString();
            }
            
            item.IsChecked = true;
            lastComItem = item;
            
        }

        public override void Cleanup()
        {
            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();
            }
            port = null;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", port.PortName);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            port.PortName =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name == typeof(double).FullName && subNode.Attributes != null)
                    .Select(subNode => subNode.Attributes[0].Value)
                    .Last();
        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    if (((Value.Number)args[0]).Item == 1)
        //    {
        //        if (port != null)
        //        {
        //            bool isOpen = true;

        //            if (isOpen == true)
        //            {
        //                if (!port.IsOpen)
        //                {
        //                    port.Open();
        //                }


        //            }
        //            else if (isOpen == false)
        //            {
        //                if (port.IsOpen)
        //                    port.Close();
        //            }
        //        }
        //    }

        //    return Value.NewContainer(port); // pass the port downstream
        //}

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            string[] serialPortNames = SerialPort.GetPortNames();

            foreach (string portName in serialPortNames)
            {

                if (lastComItem != null)
                {
                    lastComItem.IsChecked = false; // uncheck last checked item
                }
                comItem = new MenuItem { Header = portName, IsCheckable = true, IsChecked = true };
                comItem.Checked += comItem_Checked;
                nodeUI.MainContextMenu.Items.Add(comItem);

                port.PortName = portName;
                lastComItem = comItem;
            }
        }
    }

    [NodeName("Read Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Reads values from an Arduino microcontroller.")]
    public class ArduinoRead : NodeModel
    {
        SerialPort port;

        public ArduinoRead()
        {
            InPortData.Add(new PortData("arduino", "Arduino serial connection"));
            InPortData.Add(new PortData("delimiter", "The delimeter in your data coming from the Arduino."));
            OutPortData.Add(new PortData("output", "Serial output line"));

            RegisterAllPorts();
        }

        private string GetArduinoData(SerialPort port, string delim)
        {
            string data = port.ReadExisting();

            string[] allData = data.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (allData.Any())
            {
                //don't return last value. it is often truncated
                return allData[allData.Count()-2];
            }

            return string.Empty;
        }


        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    port = (SerialPort)((Value.Container)args[0]).Item;
        //    var delim = ((Value.String) args[1]).Item;

        //    var lastValue = string.Empty;

        //    if (port != null)
        //    {
        //        bool isOpen = true;

        //        if (isOpen == true)
        //        {
        //            if (!port.IsOpen)
        //            {
        //                port.Open();
        //            }

        //            //get the values from the serial port as a list of strings
        //            lastValue = GetArduinoData(port, delim);
        //        }
        //    }

        //    return Value.NewString(lastValue);
        //}

    }

    [NodeName("Write Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Writes values to an Arduino microcontroller.")]
    public class ArduinoWrite : NodeModel
    {
        SerialPort port;

        public ArduinoWrite()
        {
            InPortData.Add(new PortData("arduino", "Arduino serial connection"));
            InPortData.Add(new PortData("text", "Text to be written"));
            OutPortData.Add(new PortData("success?", "Whether or not the operation was successful."));

            RegisterAllPorts();
        }

        private void WriteDataToArduino(string dataLine)
        {

            dataLine = dataLine + "\r\n"; //termination
            port.WriteLine(dataLine);

        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{

        //    port = (SerialPort)((Value.Container)args[0]).Item;
        //    string dataToWrite = ((Value.String)args[1]).Item;// ((Value.Container)args[1]).Item;

        //    if (port != null)
        //    {
        //        bool isOpen = true;// Convert.ToBoolean(InPortData[0].Object);

        //        if (isOpen == true)
        //        {
        //            if (!port.IsOpen)
        //            {
        //                port.Open();
        //            }

        //            //write data to the serial port
        //            WriteDataToArduino(dataToWrite);

        //        }
        //        else if (isOpen == false)
        //        {
        //            if (port.IsOpen)
        //                port.Close();
        //        }
        //    }
            

        //    return Value.NewNumber(1);// catch failures here 
        //}
    }
}
