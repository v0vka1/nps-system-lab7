NPS MONITOR — клієнтська частина для лабораторної роботи №7

Файли:
- index.html — сторінка вебклієнта
- styles.css — оформлення
- app.js — запити до Nps.API

Адреса API вже налаштована в app.js:
https://nps-system-lab7.onrender.com

РОЗГОРТАННЯ НА RENDER STATIC SITE
1. Розпакуйте папку Nps.Web у корінь вашого GitHub-репозиторію поруч із Nps.API, Nps.Domain тощо.
2. Зробіть commit і push у GitHub.
3. У Render: New + → Static Site → виберіть репозиторій.
4. Root Directory: Nps.Web
5. Build Command: залишити порожнім.
6. Publish Directory: .
7. Створіть Static Site і скопіюйте його адресу https://....onrender.com.

ПІСЛЯ СТВОРЕННЯ STATIC SITE НАЛАШТУЙТЕ CORS В API
Перед builder.Build() у Nps.API/Program.cs зареєструйте:

builder.Services.AddCors(options =>
{
    options.AddPolicy("NpsWeb", policy =>
        policy.WithOrigins("https://ВАША-АДРЕСА-STATIC-SITE.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

Після builder.Build() та перед app.MapControllers() додайте:

app.UseCors("NpsWeb");

Якщо в Program.cs уже є app.UseAuthorization(), поставте app.UseCors("NpsWeb") ПЕРЕД ним.
Після зміни зробіть commit/push і дочекайтеся успішного повторного deploy API.

API маршрути, використані клієнтом:
- GET /api/Devices
- POST /api/Devices
- GET /api/Devices/{id}
- PUT /api/Devices/{id}
- DELETE /api/Devices/{id}
- PATCH /api/Devices/{id}/status з тілом { "isActive": true або false }

Секрети й connection strings у файлах відсутні. Інтерфейс є навчальним засобом демонстрації CRUD над записами бази даних, а не керуванням реальним обладнанням.
