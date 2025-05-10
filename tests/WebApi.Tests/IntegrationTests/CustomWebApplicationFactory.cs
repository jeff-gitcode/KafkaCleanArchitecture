using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;

namespace WebApi.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the KafkaConsumerService
                var kafkaConsumerDescriptor = services.SingleOrDefault(
                    d => d.ImplementationType == typeof(KafkaConsumerService));
                if (kafkaConsumerDescriptor != null)
                {
                    services.Remove(kafkaConsumerDescriptor);
                }

                // Remove IKafkaProducer if it exists
                var kafkaProducerDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IKafkaProducer));
                if (kafkaProducerDescriptor != null)
                {
                    services.Remove(kafkaProducerDescriptor);
                }

                // Add a mock IKafkaProducer
                services.AddSingleton<IKafkaProducer, MockKafkaProducer>();

                // Add an in-memory database for testing
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Ensure the database is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }

    // Mock KafkaProducer implementation
    public class MockKafkaProducer : IKafkaProducer
    {
        public Task ProduceAsync<T>(string topic, string key, T message)
        {
            return Task.CompletedTask;
        }
    }
}