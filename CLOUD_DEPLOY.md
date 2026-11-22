# Підготовка до розгортання Nps.API у хмарі (інструкція, розгортання не виконувалось)

Платформа: Azure App Service (Web App for Containers) або Render (Docker).
Варіант розгортання: контейнер, зібраний з Dockerfile цього проєкту.

Змінні середовища платформи (у коді секретів немає):
- ASPNETCORE_URLS = http://+:8080
- WEBSITES_PORT = 8080 (для Azure App Service)
- ConnectionStrings__NpsDatabase = Data Source=/home/nps.db (постійний каталог платформи)
  Для керованої БД (Azure SQL / PostgreSQL) рядок підключення з паролем зберігається
  в Application settings або Key Vault, а не в коді чи образі.

HTTPS: на платформі вмикається «HTTPS Only»; зовнішній трафік іде через HTTPS.
Health-check: шлях /health (застосунок) і /health/db (база даних).
Журнали: стандартний вивід контейнера (Log stream / Application Insights).

Перевірка після розгортання: /health, /swagger, POST/GET/PUT/PATCH/DELETE для /api/devices.