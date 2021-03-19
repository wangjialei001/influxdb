using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Msg.Core.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Msg.Core.MQ
{
    public class MQUtil
    {
        private readonly static string url = string.Empty;
        public static void Init()
        {

        }
        static MQUtil()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("msgCoreConfig.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
            url = configuration["MQ:Url"];
            producerDic = new ConcurrentDictionary<string, IProducer<Null, string>>();
        }

        public static async Task Produce(string topic, string message)
        {
            try
            {
                IProducer<Null, string> p;
                if (!producerDic.TryGetValue(topic, out p) || p == null)
                {
                    var config = new ProducerConfig { BootstrapServers = url };
                    p = new ProducerBuilder<Null, string>(config).Build();
                    producerDic.TryAdd(topic, p);
                }
                else
                {
                    p = producerDic[topic];
                }
                try
                {
                    var dr = await p.ProduceAsync(topic, new Message<Null, string> { Value = message });
                    //Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
                }
                catch (ProduceException<Null, string> e)
                {
                    throw e;
                }
                //var config = new ProducerConfig { BootstrapServers = url };
                //using (var p = new ProducerBuilder<Null, string>(config).Build())
                //{
                //    try
                //    {
                //        var dr = await p.ProduceAsync(topic, new Message<Null, string> { Value = message });
                //        Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
                //    }
                //    catch (ProduceException<Null, string> e)
                //    {

                //        throw e;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delivery failed: {ex.Message}");
                throw;
            }
        }

        public static void Consume<T>(string topic, Action<string> action, string groupId = "consumer-group")
        {
            var conf = new ConsumerConfig
            {
                GroupId = groupId,
                BootstrapServers = url,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                c.Subscribe(topic);

                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true; // prevent the process from terminating.
                    cts.Cancel();
                };

                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = c.Consume(cts.Token);
                            Console.WriteLine($"Consumed message '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'.");
                            string msg = cr.Message.Value;
                            if (!string.IsNullOrEmpty(msg))
                            {
                                //var msgObj = JsonConvert.DeserializeObject<T>(msg);
                                action(msg);
                            }
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occured: {e.Error.Reason}");
                            //action(default(T));
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    c.Close();
                }
            }
        }

        public static void Consume1(string topic, Action<string> action, string groupId = "consumer-group")
        {
            var conf = new ConsumerConfig
            {
                GroupId = groupId,
                BootstrapServers = url,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true
            };
            using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                c.Subscribe(topic);

                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true; // prevent the process from terminating.
                    cts.Cancel();
                };

                try
                {
                    while (true)
                    {
                        string value = string.Empty;
                        try
                        {
                            var cr = c.Consume(cts.Token);
                            //Console.WriteLine($"Consumed message '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'.");
                            value = cr.Message.Value;
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occured: {e.Error.Reason}");
                        }
                        action(value);
                    }
                }
                catch (OperationCanceledException)
                {
                    c.Close();
                }
            }
        }
        private static ConcurrentDictionary<string, IProducer<Null, string>> producerDic;
    }
}
