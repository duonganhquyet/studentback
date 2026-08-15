using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Infrastructure.Persistence;
using StudentAcademicManagement.Infrastructure.Services;
using System.Text;
using StudentAcademicManagement.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình CORS (Hỗ trợ linh hoạt cho cả Local Dev và Cloud Deploy trên Vercel / Render)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var frontendUrl = builder.Configuration["FRONTEND_URL"];
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            policy.WithOrigins(frontendUrl, "http://localhost:5173", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// 2. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Dependency Injection
builder.Services.AddScoped<IAuthService, AuthService>();

// 4. JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Cấu hình Swagger để nhập JWT Token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Student Academic Management API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token theo format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
// Thêm HttpContextAccessor để lấy thông tin User đang đăng nhập từ Token
builder.Services.AddHttpContextAccessor();

// Đăng ký Services cho Nghiệp vụ 3
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<ISchoolService, SchoolService>();

// Email Queue & Background Sender (FIFO)
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailBackgroundSender>();

builder.Services.AddScoped<ISchoolAdminService, SchoolAdminService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IStudentProfileService, StudentProfileService>();
builder.Services.AddScoped<IStudentDocumentService, StudentDocumentService>();
builder.Services.AddScoped<IOcrService, TesseractOcrService>();
builder.Services.AddScoped<IStudentContactService, StudentContactService>();
builder.Services.AddScoped<IStudentIdentityService, StudentIdentityService>();
builder.Services.AddScoped<ISpecialCategoryService, SpecialCategoryService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPaperRequestService, PaperRequestService>();
var app = builder.Build();

// Tự động Migrate Database khi khởi chạy Server
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    // Tự động làm sạch các bản ghi thông tin gia đình bị trùng lặp (giữ bản ghi mới nhất)
    var familyMembers = dbContext.FamilyMembers.ToList();
    var duplicatesToRemove = new List<StudentFamilyMember>();

    var groupedByStudent = familyMembers.GroupBy(f => f.StudentId);
    foreach (var group in groupedByStudent)
    {
        var uniqueMembers = new List<StudentFamilyMember>();
        // Lấy bản ghi mới nhất trước (theo Id)
        foreach (var m in group.OrderByDescending(f => f.Id))
        {
            if (!uniqueMembers.Any(u => 
                u.FullName.Trim().Equals(m.FullName.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(u.PhoneNumber) && !string.IsNullOrWhiteSpace(m.PhoneNumber) && u.PhoneNumber == m.PhoneNumber)))
            {
                uniqueMembers.Add(m);
            }
            else
            {
                duplicatesToRemove.Add(m);
            }
        }
    }
    
    if (duplicatesToRemove.Any())
    {
        dbContext.FamilyMembers.RemoveRange(duplicatesToRemove);
        dbContext.SaveChanges();
        Console.WriteLine($"[Cleanup] Deleted {duplicatesToRemove.Count} duplicate family members.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var webRootPath = builder.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRootPath),
    RequestPath = ""
});
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();