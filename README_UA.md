# Лабораторна робота №4 — NpsSystem (REST API)

Цей проєкт продовжує попередню лабораторну №3: збережено назви `Nps.Domain`, `Nps.Infrastructure`, `Nps.Services`, `Nps.API`, сутність `PlantDevice`, перелік `DeviceType`, інтерфейси `IDeviceRepository` та `IDeviceService`, а також сервіс `DeviceService`.

## Вимоги

- Windows 10/11
- .NET SDK 8.0
- Visual Studio Code (розширення C# / C# Dev Kit бажане)
- Доступ до NuGet під час першого `dotnet restore` (для Swashbuckle/Swagger)

## Запуск

1. Розпакувати ZIP в окрему папку, наприклад `D:\Labs\NpsSystem_Lab4`.
2. Відкрити VS Code → File → Open Folder → вибрати папку `NpsSystem_Lab4`.
3. Відкрити Terminal → New Terminal (PowerShell).
4. Перевірити SDK:

   ```powershell
   dotnet --version
   ```

   Має відобразитися версія 8.x.

5. Відновити пакети та зібрати рішення:

   ```powershell
   dotnet restore .\NpsSystem.sln
   dotnet build .\NpsSystem.sln
   ```

6. Запустити API:

   ```powershell
   dotnet run --project .\Nps.API\Nps.API.csproj
   ```

7. У браузері відкрити Swagger:

   `http://localhost:5124/swagger`

   API використовує фіксований локальний порт 5124. Якщо порт зайнятий, зупинити попередній екземпляр API або змінити `applicationUrl` у `Nps.API/Properties/launchSettings.json` і надалі використовувати новий порт.

## Початкові навчальні записи

Під час запуску API до сховища додаються два тестові пристрої:

- `Насос аварійного охолодження` (`Actuator`)
- `Датчик температури активної зони` (`Sensor`)

Це локальні демонстраційні записи, а не дані реального обладнання. Репозиторій `InMemoryDeviceRepository` є Singleton, тому записи зберігаються між HTTP-запитами, але зникають після перезапуску процесу.

## Ендпоїнти

| Метод | URL | Призначення | Очікувана відповідь |
|---|---|---|---|
| GET | `/api/devices` | Усі пристрої | 200 |
| GET | `/api/devices?name=насос` | Фільтр за назвою | 200 |
| GET | `/api/devices/{id}` | Один пристрій | 200 / 404 |
| POST | `/api/devices` | Створення | 201 / 400 |
| PUT | `/api/devices/{id}` | Повне оновлення назви й типу | 200 / 400 / 404 |
| PATCH | `/api/devices/{id}/status` | Задати активний стан | 204 / 404 |
| DELETE | `/api/devices/{id}` | Видалення | 204 / 404 |

### JSON для POST

```json
{
  "name": "Насос резервного охолодження",
  "type": "Actuator"
}
```

### JSON для PUT

```json
{
  "name": "Насос резервного охолодження №2",
  "type": "Actuator"
}
```

### JSON для PATCH

```json
{
  "isActive": true
}
```

Для деактивації надіслати `false`. PATCH передає бажаний стан, а не команду «перемкнути», тому повторний запит із тим самим значенням не змінює результат.

## Важливе про реалізацію

- Контролер не звертається до репозиторію безпосередньо. Ланцюжок: `DevicesController → IDeviceService → IDeviceRepository`.
- `InMemoryDeviceRepository` використовує `List<PlantDevice>` та блокування `lock` для операцій зі списком.
- `ConcurrentDeviceRepository` з лабораторної №3 також оновлений методами `UpdateAsync` і `DeleteAsync`, але у цій лабораторній через DI вибрано `InMemoryDeviceRepository`, щоб безпосередньо виконати вимогу методички щодо InMemory сховища.
- Swagger UI та XML-коментарі підключені в `Program.cs` і `Nps.API.csproj`.

## Послідовність скриншотів

Зробити скриншоти після виконання відповідних кроків (не використовувати макети або підроблені результати):

1. Структура рішення в Explorer: чотири проєкти `Nps.Domain`, `Nps.Infrastructure`, `Nps.Services`, `Nps.API`.
2. `IDeviceRepository.cs` — усі п'ять методів, зокрема `UpdateAsync` і `DeleteAsync`.
3. `PlantDevice.cs` — конструктор, `Activate`, `Deactivate`, `UpdateDetails`.
4. `InMemoryDeviceRepository.cs` — `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.
5. `CreateDeviceRequest.cs` і `UpdateDeviceRequest.cs` — атрибути Data Annotations.
6. `IDeviceService.cs` — контракт сервісних операцій.
7. `DeviceService.cs` — пошук, створення, оновлення, встановлення стану, видалення і мапінг DTO.
8. `DevicesController.cs` — усі сім HTTP-операцій/маршрутів.
9. `Program.cs` — Swagger, DI і тестові записи.
10. Terminal з успішним `dotnet build`.
11. Terminal із запущеним API та адресою `http://localhost:5124`.
12. Swagger UI зі списком ендпоїнтів і Schemas.
13. GET `/api/devices` — список двох початкових пристроїв.
14. GET `/api/devices?name=насос` — фільтр за назвою.
15. GET `/api/devices/{id}` — успішне отримання (200).
16. GET `/api/devices/{невідомий-guid}` — 404.
17. POST — успішне створення з відповіддю 201 Created і Location.
18. POST — порожня назва, відповідь 400 Bad Request.
19. PUT — оновлення пристрою, відповідь 200 OK.
20. PATCH з `isActive: true`, відповідь 204 No Content; бажано повторити PATCH з тим самим значенням.
21. DELETE наявного пристрою — 204 No Content.
22. Повторний DELETE того самого Id — 404 Not Found.

Для тестів у Swagger натискати `Try it out` → вводити дані → `Execute`. Для операцій, яким потрібен `id`, скопіювати GUID із відповіді GET або POST. Після DELETE повторити запит саме з тим самим GUID.

Після виконання кожного блоку надіслати скриншоти. Фінальний звіт оформлюватиметься за реальними результатами та скриншотами, які будуть надані.
