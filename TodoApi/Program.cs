using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

//הוספת סרויסים לשרת
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // מאפשר לכל מקור לגשת (למשל: כל הדומיינים)
              .AllowAnyHeader()  // מאפשר כל כותרת
              .AllowAnyMethod(); // מאפשר כל סוג של בקשה (GET, POST, PUT, DELETE וכו')
    });
});

builder.Services.AddDbContext<to_do_tasksContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("to_do_tasks"),
                     Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Authorization with JWT",
        Type =SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"] ?? throw new InvalidOperationException("JWT:Audience is not configured"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is not configured")))
    };
});

var app = builder.Build();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
object CreateJWT(User user, IConfiguration configuration){
    var claims = new List<Claim>()
    {
        new Claim ("id", user.Id.ToString()),
        new Claim ("NameUser", user.NameUser),
    };
    var key = configuration["JWT:Key"] ?? "default_secret_key";
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
    var tokeOption = new JwtSecurityToken(
        issuer: configuration.GetValue<string>("JWT:Issuer"),
        audience: configuration.GetValue<string>("JWT:Audience"),
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: credentials
    );
    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOption);
    return new {Token = tokenString};
}

//בזמן פיתוח - שימוש בswagger
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

//פונקציית התחברות
app.MapPost("/login", async (User loginRequest, HttpContext context, to_do_tasksContext tasksData,  IConfiguration configuration) =>
{
    if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.NameUser) || string.IsNullOrWhiteSpace(loginRequest.password))
    {
        return Results.BadRequest("שם משתמש או סיסמה לא הוזנו.");
    }
    var user = tasksData.Users?.FirstOrDefault(u => u.NameUser == loginRequest.NameUser && u.password == loginRequest.password);

    if (user == null)
    {
        return Results.Unauthorized();
    }
    Console.WriteLine($"NameUser: {loginRequest.NameUser}, Password: {loginRequest.password}");
    var jwt = CreateJWT(user, configuration);
    Console.WriteLine(jwt);
    return Results.Ok(new { Token = jwt });
});

//פונקציית הרשמה
app.MapPost("/registr", async (User registrRequest, HttpContext context, to_do_tasksContext tasksData,  IConfiguration configuration) =>
{
    app.Logger.LogInformation("Login attempt for user: {NameUser}", registrRequest.NameUser); // לוג עבור התחברות
    if (registrRequest == null || string.IsNullOrWhiteSpace(registrRequest.NameUser) || string.IsNullOrWhiteSpace(registrRequest.password))
    {
                app.Logger.LogWarning("Invalid login request."); // לוג אזהרה אם הבקשה לא תקינה
        return Results.BadRequest("שם משתמש או סיסמה לא הוזנו.");
    }
    var user = tasksData.Users?.FirstOrDefault(u => u.NameUser == registrRequest.NameUser);

    if (user != null)
    {
        app.Logger.LogInformation("לא נמצא");
        return Results.BadRequest("המשתמש כבר קיים");
    }
   app.Logger.LogInformation("הרשמה ממשיכה");
    var newUser = new User{
        NameUser = registrRequest.NameUser,
        password = registrRequest.password
    };
    tasksData.Add(newUser);
    await tasksData.SaveChangesAsync(); // שמירת השינויים במסד הנתונים
    var jwt = CreateJWT(newUser, configuration);
    Console.WriteLine(jwt);
        app.Logger.LogInformation("הרשמה נגמרת");
    return Results.Ok(new { Token = jwt });
});

//פונקציה בסיסית
app.MapGet("/", () =>{
            app.Logger.LogInformation("לוג שעובד");
     return "ברוכים הבאים";});

//פונקציה לניסיון הטלפון
app.MapGet("/phon", () => {
    app.Logger.LogInformation("לוג שעובד");
    return "welcome";
});

//שליפת כל המשימות
app.MapGet("/tasks",[Authorize]   async (int id, HttpContext context, to_do_tasksContext tasksData) =>
{
    
    // שליפת כל המשימות ששייכות למשתמש
    var userTasks = await tasksData.Items
        .Where(item => item.UserId == id) // סינון לפי UserId
        .ToListAsync();
    
    return Results.Ok(userTasks);
});

//הוספת משימה חדשה
app.MapPost("/tasks",[Authorize]  async (String newTask, int id, HttpContext context, to_do_tasksContext tasksData) =>
{
   Console.WriteLine(newTask);
    if (string.IsNullOrWhiteSpace(newTask))
    {
        return Results.BadRequest("Task name cannot be empty");
    }

    var newItem = new Item
    {
        Name = newTask,
        IsComplete = false,
        UserId = id
    };

    tasksData.Items.Add(newItem);
    await tasksData.SaveChangesAsync();

    return Results.Ok(newItem);
});

//עדכון משימה
app.MapPut("/tasks/{id}", [Authorize]  async (int id, to_do_tasksContext tasksData) =>
{
    var item = await tasksData.Items.FindAsync(id);
    if (item != null)
        item.IsComplete = !item.IsComplete;
    await tasksData.SaveChangesAsync();
});

//מחיקת משימה
app.MapDelete("/tasks/{id}", [Authorize]  async (int id, to_do_tasksContext tasksData) =>
{
    var item = await tasksData.Items.FindAsync(id);
    if (item != null)
        tasksData.Remove(item);
    await tasksData.SaveChangesAsync();

});

app.Run();




