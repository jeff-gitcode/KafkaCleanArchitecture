using Application.Interfaces;
using Infrastructure.Kafka;
using Infrastructure.Persistence;
using Microsoft.OpenApi.Models;
using Application.Commands;
using MediatR;
using Application.Queries;
using Microsoft.EntityFrameworkCore;
using Domain;
using Application;
using Infrastructure;

namespace WebApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Register MediatR
            // services.AddMediatR(typeof(CreateEntityCommand).Assembly);
            // services.AddMediatR(typeof(GetEntityQuery).Assembly);
            // Add Domain Layer Services (if any)
            services.AddDomainServices();

            // Add Application Layer Services
            services.AddApplicationServices();

            // Add Infrastructure Layer Services
            var connectionString = "InMemoryDb"; // Replace with your actual connection string if needed
            var kafkaBootstrapServers = "localhost:9093";  //"localhost:9092"; // Replace with your Kafka server address
            services.AddInfrastructureServices(connectionString, kafkaBootstrapServers);

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Kafka Clean Architecture API",
                    Version = "v1"
                });
            });

            // // Provide the bootstrapServers value for KafkaProducer
            // var bootstrapServers = "localhost:9092"; // Replace with your Kafka server address
            // services.AddScoped<IKafkaProducer>(provider => new KafkaProducer(bootstrapServers));

            // services.AddDbContext<AppDbContext>(options =>
            //     options.UseInMemoryDatabase("InMemoryDb"));

            // // Add other services and configurations as needed
            // services.AddScoped<IEntityRepository, EntityRepository>();

            // // Register the Kafka consumer as a hosted service
            // services.AddHostedService<KafkaConsumerService>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Enable Swagger in Development
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kafka Clean Architecture API v1");
                    c.RoutePrefix = string.Empty; // Swagger UI at the root
                });
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Start Kafka Consumer
            // using (var scope = app.ApplicationServices.CreateScope())
            // {
            //     var serviceProvider = scope.ServiceProvider;

            //     var kafkaConsumer = new KafkaConsumer("entity-topic", "localhost:9092", serviceProvider);
            //     var cancellationTokenSource = new CancellationTokenSource();
            //     Task.Run(() => kafkaConsumer.StartConsuming(cancellationTokenSource.Token));
            // }
        }
    }
}