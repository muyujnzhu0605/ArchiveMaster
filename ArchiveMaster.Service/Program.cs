using ArchiveMaster.Configs;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace ArchiveMaster.Service;

internal class Program
{
    private static bool swagger = true;

    private static WebApplication app;
    private static string cors = "cors";

    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = CreateBuilder(args);

        app = builder.Build();

        SettingApp(app);

        app.Run();
    }

    private static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            Args = args,
            ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default,
        });

        // Add services to the container.

        var mvcBuilder = builder.Services.AddControllers(o =>
               {
                   o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                   o.Filters.Add(app.Services.GetRequiredService<ArchiveMasterActionFilter>());
               })
               .AddJsonOptions(o => { o.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All); });

        new Initializer().Initialize(builder.Services, mvcBuilder);

        builder.Services.AddTransient<ArchiveMasterActionFilter>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        // builder.Services.AddSwaggerGen(p =>
        // {
        //     var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
        //     var xmlPath = Path.Combine(basePath, "ArchiveMaster.Service.xml");
        //     p.IncludeXmlComments(xmlPath);
        //
        //     var scheme = new OpenApiSecurityScheme()
        //     {
        //         Description = "Authorization header",
        //         Reference = new OpenApiReference
        //         {
        //             Type = ReferenceType.SecurityScheme,
        //             Id = "Authorization"
        //         },
        //         Scheme = "oauth2",
        //         Name = "Authorization",
        //         In = ParameterLocation.Header,
        //         Type = SecuritySchemeType.ApiKey,
        //     };
        //     p.AddSecurityDefinition("Authorization", scheme);
        //     var requirement = new OpenApiSecurityRequirement();
        //     requirement[scheme] = new List<string>();
        //     p.AddSecurityRequirement(requirement);
        // });

        builder.Services.AddHostedService<AppLifetimeService>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: cors,
                policy =>
                {
                    policy.AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyOrigin();
                });
        });
        builder.Host.UseWindowsService();
        // builder.Host.UseWindowsService(c => { c.ServiceName = "ArchiveMaster"; });
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        return builder;
    }

    private static void SettingApp(WebApplication app)
    {
        if (swagger || app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(o => { });
        }

        //app.UseWebSockets();
        app.UseHttpsRedirection();
        app.UseCors(cors);
        //app.UseAuthorization();
        app.MapControllers();
    }
}