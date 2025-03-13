using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;

class Program
{
    static void Main()
    {
        Console.WriteLine("JSON TCP Client started.");

        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine("Select method (Random, Add, Subtract):");
            string method = Console.ReadLine();

            Console.WriteLine("Enter two numbers separated by a space (e.g., 5 10):");
            string numbersInput = Console.ReadLine();
            string[] nums = numbersInput.Split(' ');

            if (nums.Length != 2 || !int.TryParse(nums[0], out int num1) || !int.TryParse(nums[1], out int num2))
            {
                Console.WriteLine("Invalid numbers entered, please retry.");
                i--;
                continue;
            }

            // Create JSON request
            var jsonObject = new { method, num1, num2 };
            string jsonRequest = JsonSerializer.Serialize(jsonObject);

            // Single connection per request
            using TcpClient client = new TcpClient("127.0.0.1", 6000);
            using StreamReader reader = new StreamReader(client.GetStream());
            using StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

            writer.WriteLine(jsonRequest);

            string jsonResponse = reader.ReadLine();

            // Parse JSON response
            try
            {
                JsonDocument responseDoc = JsonDocument.Parse(jsonResponse);
                var root = responseDoc.RootElement;
                string status = root.GetProperty("status").GetString();

                if (status == "OK")
                {
                    int result = root.GetProperty("result").GetInt32();
                    Console.WriteLine($"Server result: {result}");
                }
                else if (status == "Error")
                {
                    string message = root.GetProperty("message").GetString();
                    Console.WriteLine($"Server Error: {message}");
                }
            }
            catch
            {
                Console.WriteLine("Received invalid JSON response from server.");
            }
        }

        Console.WriteLine("Finished.");
    }
}