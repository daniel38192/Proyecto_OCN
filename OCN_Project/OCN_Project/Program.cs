using Microsoft.EntityFrameworkCore;
using OCN_Project.Data;

var builder = WebApplication.CreateBuilder(args);

// SuppressImplicitRequiredAttributeForNonNullableReferenceTypes:
// evita que MVC marque como [Required] las propiedades de navegación no-nulables
// (Division, TipoZona, Categoria, etc.). Sin esto, los POST de entidades con
// relaciones fallan en ModelState aunque el formulario sea válido, porque el
// formulario solo envía las llaves foráneas (IdDivision, IdTipoZona, ...),
// no las propiedades de navegación.
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Entity Framework Core -> base proyecto_ocn en localhost (autenticación de Windows).
builder.Services.AddDbContext<OcnDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OcnConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();