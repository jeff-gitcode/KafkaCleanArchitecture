using Application.Interfaces;
using Infrastructure.Kafka;
using Infrastructure.Persistence;
using Microsoft.OpenApi.Models;
using Application.Commands;
using MediatR;
using Application.Queries;

namespace WebApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Register MediatR
            services.AddMediatR(typeof(CreateEntityCommand).Assembly);
            services.AddMediatR(typeof(GetEntityQuery).Assembly);

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Kafka Clean Architecture API",
                    Version = "v1"
                });
            });

            // Provide the bootstrapServers value for KafkaProducer
            var bootstrapServers = "localhost:9092"; // Replace with your Kafka server address
            services.AddScoped<IKafkaProducer>(provider => new KafkaProducer(bootstrapServers));

            services.AddDbContext<AppDbContext>();
            // Add other services and configurations as needed
            // Add other services and configurations as needed
            services.AddScoped<IEntityRepository, EntityRepository>();
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
            var kafkaConsumer = new KafkaConsumer("entity-topic", "localhost:9092");
            var cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => kafkaConsumer.StartConsuming(cancellationTokenSource.Token));

        }
    }
}