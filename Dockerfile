# ===== Tahap 1: build (pakai image SDK yang berisi compiler .NET) =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore package dulu (layer ini di-cache selama .csproj tidak berubah)
COPY BelajarAspNetMvc.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app --no-restore

# ===== Tahap 2: runtime (image kecil, hanya untuk menjalankan) =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_ENVIRONMENT=Production
# Render memberi nomor port lewat variabel PORT; default 8080 kalau dijalankan di tempat lain
CMD ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet BelajarAspNetMvc.dll"]
