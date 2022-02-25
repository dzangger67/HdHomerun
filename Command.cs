/*
    The MIT License (MIT)
    Copyright © 2022 David Zangger

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
    and associated documentation files (the “Software”), to deal in the Software without 
    restriction, including without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom 
    the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or 
    substantial portions of the Software.

    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
    PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
    FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
    OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
    DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdHomerun
{
    /// <summary>
    /// The command class is used to help parse the command the user enters at the prompt.
    /// </summary>
    internal class Command
    {
        public string Object { get; set; }      // serials, channels
        public int? Seq { get; set; }           // the sequence 
        public string Action { get; set; }      // del, keep, clean
        public int? Count { get; set; }         // 0 = all, 1, 2, 3 ...
        public bool Valid { get; set; }         // was this commmand parsed ok?
        public bool Help { get; set; }          // Show help
        public bool All { get; set; }           // Used the * to mean all

        /// <summary>
        /// Shows the command parsed into a readable string
        /// </summary>
        /// <returns>the command in an easily readable string</returns>
        public override string ToString()
        {
            string seq = (Seq == null ? "null" : Seq.Value.ToString());
            string action = (Action == null ? "null" : Action);

            return $"Obj [{Object}]  Seq [{seq}]  All [{All}] Action [{action}]  Count [{Count}]";
        }
        public Command(string command)
        {
            // Set some defaults
            Help = false;
            All = false;

            // Examples of some commands the user can enter

            //  ser							# list all the serials
            //  ser 2						# show recordings for the serial
            //  ser ?                       # show help for this object
            //  ser *                       # show all recordings for all the series
            //  ser 2 delete    1|*			# delete recording #1 for serial #2
            //  ser 2 keep      4			# keep up to 4 recordings for serial #2 
            //  ser 2 clean					# clean up recordings for serial #2
            string[] parts = command.Split(' ');
            try
            {
                // set the object
                if (parts.Length > 0)
                    Object = parts[0];

                // The sequence # of the object
                if (parts.Length > 1)
                {
                    if (parts[1] == "?")
                    {
                        Help = true;
                    }
                    else if (parts[1] == "*")
                    {
                        Seq = null;
                        All = true;
                    }
                    else
                        Seq = int.Parse(parts[1]);
                }
                else
                {
                    Seq = null;
                    All = false;
                }

                if (parts.Length > 2)
                {
                    Action = parts[2];
                }
                else
                {
                    Action = null;
                }

                if (parts.Length > 3)
                {
                    if (parts[3] == "*")
                    {
                        Count = null;
                        All = true;
                    }
                    else
                        Count = int.Parse(parts[3]);
                }
                else
                {
                    Count = null;
                }

                // Set valid for now
                Valid = true;
            }
            catch (Exception)
            {
                Valid = false;
            }          
        }
    }
}
