using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 6000);
        listener.Start();
        Console.WriteLine("JSON TCP Server started on port 6000.");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("New client connected.");

            Task.Run(() => HandleClient(client));
        }
    }

    static void HandleClient(TcpClient client)
    {
        using StreamReader reader = new StreamReader(client.GetStream());
        using StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

        try
        {
            string jsonRequest = reader.ReadLine();
            Console.WriteLine($"Received JSON: {jsonRequest}");

            JsonDocument requestDoc = JsonDocument.Parse(jsonRequest);
            var root = requestDoc.RootElement;

            if (root.TryGetProperty("method", out JsonElement methodElement) &&
                root.TryGetProperty("num1", out JsonElement num1Element) &&
                root.TryGetProperty("num2", out JsonElement num2Element))
            {
                string method = methodElement.GetString();
                int num1 = num1Element.GetInt32();
                int num2 = num2Element.GetInt32();
                int result;

                switch (method.ToLower())
                {
                    case "random":
                        Random rnd = new Random();
                        result = rnd.Next(Math.Min(num1, num2), Math.Max(num1, num2) + 1);
                        break;
                    case "add":
                        result = num1 + num2;
                        break;
                    case "subtract":
                        result = num1 - num2;
                        break;
                    default:
                        writer.WriteLine(JsonSerializer.Serialize(new { status = "Error", message = "Invalid method" }));
                        return;
                }

                writer.WriteLine(JsonSerializer.Serialize(new { status = "OK", result }));
                Console.WriteLine($"Sent result: {result}");
            }
            else
            {
                writer.WriteLine(JsonSerializer.Serialize(new { status = "Error", message = "Invalid JSON format or missing parameters" }));
            }
        }
        catch (Exception ex)
        {
            writer.WriteLine(JsonSerializer.Serialize(new { status = "Error", message = ex.Message }));
        }
        finally
        {
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
}