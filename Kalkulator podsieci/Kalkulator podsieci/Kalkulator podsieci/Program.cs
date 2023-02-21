using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Kalkulator_podsieci
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Witaj! \r\nZostałem stworzony, do obliczania podsieci :)");
            Console.WriteLine("");
            while (true)
            {

                Console.WriteLine("");
                Console.WriteLine("------------------------------------------");
                string ipString = GetUserInput("Podaj adres ip do podziału:");
                string onesInNetmaskString = GetUserInput("Podaj ilość jedynek w masce:");
                string numberOfSubnetsString = GetUserInput("Podaj ilość podsieci");

                                       
                int onesInNetmask = ParseStringToInt(onesInNetmaskString);
                CheckIfNumberOfOnesInNetmaskIsCorrect(onesInNetmask);                  
                int numberOfSubnets = ParseStringToInt(numberOfSubnetsString);

                CheckIfNumberOfSubnetsIsCorrect(numberOfSubnets);

                Console.WriteLine("Adres sieci do podziału: \b\n " + ipString);

                
                int[] individualIpOctets = GetIndividualOctets(ipString);
              
                int[,] binaryNetmask = GetBinaryNetmask(onesInNetmask);

                string[] binaryNetmaskString = ConvertBinaryNetmaskToString(binaryNetmask);

                int[] netmask = ConvertBinaryNetmaskToDecimal(binaryNetmaskString); 

   

                WriteNetmask("Maska zadanej podsieci:", netmask);
                WriteNetmask("Maska podsieci w systemie binarnym:", binaryNetmaskString);

                int numberOfZerosToSwitch = CalculateNumberOfZerosToSwitch(numberOfSubnets);

                SwitchCorrectAmountOfZerosInNetmask(numberOfZerosToSwitch, ref binaryNetmask);


                Array.Clear(binaryNetmaskString, 0, binaryNetmaskString.Length);
                binaryNetmaskString = ConvertBinaryNetmaskToString(binaryNetmask);
                
                WriteNetmask("Nowa maska podsieci w systemie binarnym:", binaryNetmaskString);


                Array.Clear(netmask, 0, netmask.Length);
                netmask = ConvertBinaryNetmaskToDecimal(binaryNetmaskString);

                WriteNetmask("Nowa maska podsieci w systemie dziesiętnym:", netmask);


                int powerOfTwo = CalculatePowerOfTwoToAdd(numberOfZerosToSwitch);

                int octetIndex = GetFirstOctetWithZero(individualIpOctets);

                int[,] subnets = CalculateSubnets(numberOfSubnets, individualIpOctets, octetIndex, powerOfTwo);

                WriteAdresses("Adresy podsieci:", numberOfSubnets, subnets);
           
                int[,] broadcasts = subnets.Clone() as int[,];
                broadcasts = CalculateBroadcasts(numberOfSubnets, ref broadcasts);

                WriteAdresses("Adres rozgłoszeniowy:", numberOfSubnets, broadcasts);


                int[,] startingAddresses = subnets.Clone() as int[,];
                startingAddresses = CalculateStartingAdresses(numberOfSubnets, ref startingAddresses);

                WriteAdresses("Adres początkowy:", numberOfSubnets, startingAddresses);

                int[,] endAddresses = broadcasts.Clone() as int[,];
                endAddresses = CalculateEndAdresses(numberOfSubnets, ref endAddresses);
                
                WriteAdresses("Adres końcowy:", numberOfSubnets, endAddresses);

            }

        }

        private static string GetUserInput(string messageToOutput)
        {
            Console.WriteLine(messageToOutput);
            return Console.ReadLine();
        }

        private static int ParseStringToInt(string text)
        {
            int number = 0;

            if (int.TryParse(text, out number))
            {
                return number;
            }
            else
            {
                WriteErrorAndStopProgram("Nie można przekonwertować tesktu na liczbę!");
                return -1;
            }            

        }

        private static void CheckIfNumberOfOnesInNetmaskIsCorrect(int onesInNetmask)
        {
            if (onesInNetmask > 32 || onesInNetmask < 0)
            {
                WriteErrorAndStopProgram("Niepoprawna ilość jedynek w masce!");
            }
        }

        private static void CheckIfNumberOfSubnetsIsCorrect(int subnets)
        {
            if (subnets == 0)
            {
                WriteErrorAndStopProgram("Niepoprawna ilość podsieci!");
            }
        }

        private static int[] GetIndividualOctets(string ip)
        {
            string[] individualIpOctetsString = SplitIpIntoIndividualOctets(ip);

            CheckIfNumberOfOctetsIsCorrect(individualIpOctetsString);


            int[] individualIpOctets = new int[4];

            for (int i = 0; i < 4; i++)
            {
                individualIpOctets[i] = ParseStringToInt(individualIpOctetsString[i]);
                CheckIfOctetIsCorrect(individualIpOctets[i]);
            }

            return individualIpOctets;
        }

        private static string[] SplitIpIntoIndividualOctets(string ip)
        {
            return ip.Split('.');
        }

        private static void CheckIfNumberOfOctetsIsCorrect(string[] individualIpOctetsString)
        {
            if (individualIpOctetsString.Length < 4)
            {
                WriteErrorAndStopProgram("Niepoprawny format adresu ip");

            }
        }

        private static void CheckIfOctetIsCorrect(int octet)
        {
            if (octet < 0 || octet > 255)
            {
                WriteErrorAndStopProgram("Niepoprawny format adresu ip");
            }
        }

        private static int[,] GetBinaryNetmask(int onesInNetmask)
        {
            int[,] binaryNetmask = new int[4, 8];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    binaryNetmask[i, j] = (onesInNetmask > 0) ? 1 : 0;
                    onesInNetmask--;
                }
            }

            return binaryNetmask;
        }

        private static string[] ConvertBinaryNetmaskToString(int[,] binaryNetmask)
        {
            string[] binaryNetmaskString = new string[4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    binaryNetmaskString[i] += binaryNetmask[i, j];
                }
            }

            return binaryNetmaskString;
        }

        private static int[] ConvertBinaryNetmaskToDecimal(string[] binaryNetmaskString)
        {
            int[] netMask = new int[4];

            for (int i = 0; i < 4; i++)
            {
                netMask[i] = Convert.ToInt32(binaryNetmaskString[i], 2);
            }
            return netMask;
        }

        private static int CalculateNumberOfZerosToSwitch(int numberOfSubnets)
        {

            int numberOfZerosToSwitch = 0;
            int result = 1;
            while (result < numberOfSubnets)
            {
                numberOfZerosToSwitch++;
                result = 1;
                for (int i = 0; i < numberOfZerosToSwitch; i++)
                {
                    result *= 2;
                }
            }

            return numberOfZerosToSwitch;
        }

        private static void SwitchCorrectAmountOfZerosInNetmask(int numberOfZerosToSwitch, ref int[,] binaryNetmask)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (binaryNetmask[i, j] == 1)
                        continue;
                    if (numberOfZerosToSwitch > 0)
                    {
                        binaryNetmask[i, j] = 1;
                    }
                    else
                        break;

                    numberOfZerosToSwitch--;
                }
            }
        }

        private static int CalculatePowerOfTwoToAdd(int numberOfZerosToSwitch)
        {
            int powerOfTwo = 128;

            for (int i = 1; i < numberOfZerosToSwitch; i++)
            {
                powerOfTwo /= 2;
            }

            return powerOfTwo;
        }

        private static int GetFirstOctetWithZero(int[] individualIpOctets)
        {
            int octetIndex = -1;

            List<int> individualIpOctetsList = new List<int>(individualIpOctets);

            octetIndex = individualIpOctetsList.FindIndex(octet => octet == 0);

            CheckIfFoundOctet(octetIndex);
            return octetIndex;
        }

        private static void CheckIfFoundOctet(int octetIndex)
        {
            if (octetIndex == -1)
                Environment.Exit(0);
        }

        private static int[,] CalculateSubnets(int numberOfSubnets, int[] individualIpOctets, int octetIndex, int powerOfTwo)
        {
            int[,] subnets = new int[numberOfSubnets, 4];

            for (int i = 0; i < numberOfSubnets; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    subnets[i, j] = individualIpOctets[j];
                }
                individualIpOctets[octetIndex] += powerOfTwo;
            }

            return subnets;
        }

        private static int[,] CalculateBroadcasts(int numberOfSubnets, ref int[,] broadcasts)
        {
            for (int i = 0; i < numberOfSubnets; i++)
            {
                if (i == numberOfSubnets - 1)
                    CalculateLastBroadcast(ref broadcasts, numberOfSubnets);
                else
                    CalculateNextBroadcast(ref broadcasts, i);
            }
            return broadcasts;
        }

        private static void CalculateLastBroadcast(ref int[,] broadcasts, int numberOfSubnets)
        {
            for (int i = 3; i >= 0; i--)
            {

                if (broadcasts[numberOfSubnets - 1, i] == 0)
                    broadcasts[numberOfSubnets - 1, i] = 255;
                else
                {
                    broadcasts[numberOfSubnets - 1, i] = 255;
                    break;
                }
            }
        }

        private static void CalculateNextBroadcast(ref int[,] broadcasts, int broadcastIndex)
        {

            ShiftBroadcastArray(ref broadcasts, broadcastIndex);


            for (int j = 3; j >= 0; j--)
            {
                if (broadcasts[broadcastIndex, j] == 0)
                    broadcasts[broadcastIndex, j] = 255;
                else
                {
                    broadcasts[broadcastIndex, j] = broadcasts[broadcastIndex, j] - 1;
                    break;
                }
            }
        }

        private static void ShiftBroadcastArray(ref int[,] broadcasts, int broadcastIndex)
        {

            for (int j = 0; j < 4; j++)
            {
                broadcasts[broadcastIndex, j] = broadcasts[broadcastIndex + 1, j];
            }
        }

        private static int[,] CalculateStartingAdresses(int numberOfSubnets, ref int[,] startingAddresses)
        {
            for (int i = 0; i < numberOfSubnets; i++)
            {
                startingAddresses[i, 3] += 1;
            }
            return startingAddresses;
        }

        private static int[,] CalculateEndAdresses(int numberOfSubnets,  ref int[,] endAddresses)
        {
            for (int i = 0; i < numberOfSubnets; i++)
            {
                endAddresses[i, 3] -= 1;

            }

            return endAddresses;
        }

        private static void WriteAdresses(string message, int numberOfSubnets, int[,] arrayToWrite)
        {
            Console.Write(message + "\b\n");
            for (int i = 0; i < numberOfSubnets; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Console.Write(arrayToWrite[i, j] + ".");
                }
                Console.WriteLine(" ");

            }

            Console.WriteLine(" ");
        }


        private static void WriteNetmask(string message, int[] netmask)
        {
            Console.Write(message + "\b\n");
            for (int i = 0; i < 4; i++)
            {
                Console.Write(netmask[i] + ".");
            }


            Console.WriteLine("");
            Console.WriteLine("");
        }

        private static void WriteNetmask(string message, string[] netmask)
        {
            Console.Write(message + "\b\n");
            for (int i = 0; i < 4; i++)
            {
                Console.Write(netmask[i] + ".");
            }


            Console.WriteLine("");
            Console.WriteLine("");
        }

        private static void WriteErrorAndStopProgram(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            Environment.Exit(0);
        }
    }
}
