using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomationUWP.Helper_classes
{
    public class PoolControler : ESP8266
    {
        private const string _turnOn = "turnOn";
        private const string _turnOff = "turnOff";

        private StackTrace stack = new StackTrace();

        /// <summary>
        /// Turns on the pool.
        /// </summary>
        public void TurnOn()
        {
            try
            {
                Write(MakeMessage(_turnOn));
            }
            catch (Exception e)
            {
                Listen();
            }
        }

        /// <summary>
        /// Turns off the pool.
        /// </summary>
        public void TurnOff()
        {
            try
            {
                Write(MakeMessage(_turnOff));
            }
            catch (Exception e)
            {
                Listen();
            }
        }

        /// <summary>
        /// Checks pool status.
        /// </summary>
        /// <returns>Pool status: 0 - OFF, 1 - ON</returns>
        public Task<int> GetPoolStatus()
        {
            Debug.WriteLine(stack.GetFrame(1).GetMethod().Name);
            string message = "getPoolStatus\n";
            try
            {
                Write(ASCIIEncoding.ASCII.GetBytes(message));
            }
            catch (Exception e)
            {
                throw e;
            }

            string response = string.Empty;
            char newCharacter;

            do
            {
                try
                {
                    newCharacter = (char)ReadByte();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                if (newCharacter != '\n')
                {
                    response += newCharacter;
                }
            } while (newCharacter != '\n');

            if (response == "true")
            {
                return Task.FromResult(1);
            }
            else
            {
                return Task.FromResult(0);
            }

        }
    }
}
