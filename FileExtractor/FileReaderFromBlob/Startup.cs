using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FileReaderFromBlob.Startup))]

namespace FileReaderFromBlob
{
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.DependencyInjection;
    using FileDataExtractService.Implementation;
    using FileDataExtractService.Interface;

    /// <summary>
    /// Startup.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Gets configuration.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// The Configure App Configuration.
        /// </summary>
        /// <param name="builder">The configurationBuilder.</param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            this.Configuration = builder.ConfigurationBuilder.Build();
        }

        /// <summary>
        /// The configure.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IConfiguration>(this.Configuration);
            builder.Services.AddTransient<IBlobWrapper, BlobWrapper>();
            builder.Services.AddTransient<IFileServices, FileService>();
        }
    }
}
