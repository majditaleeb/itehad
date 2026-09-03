# تكسي الاتحاد — نظام إدارة الحركة اليومية

تطبيق ASP.NET MVC 5 (.NET Framework 4.8) لإدارة رحلات التكسي، الزبائن،
السائقين، الحضور، المصاريف، والتقارير.

## المحتويات

| المجلد | الوصف |
|---|---|
| `bin/` | التطبيق المُترجَم والمكتبات |
| `App_Code/` | كود يُترجَم وقت التشغيل (الحذف، سجل التدقيق، تصدير Excel، حفظ الرحلات دفعة واحدة) |
| `Views/` | صفحات Razor |
| `Scripts/`, `Content/` | ملفات الواجهة |
| `Web.config` | الإعدادات وسلسلة الاتصال |

## التركيب على السيرفر

1. شغّل ملف `it-triggers.sql` على قاعدة بيانات `itehad`
   (ينشئ جدول `TripAuditLog` ومشغّلات تسجيل التعديل والحذف).
2. انسخ محتويات المستودع إلى مجلد التطبيق في IIS.
3. عدّل اسم خادم SQL في `Web.config` إن لزم:

   ```xml
   <add name="ApplicationDbContext"
        connectionString="Server=.\SQLEXPRESS;Database=itehad;Trusted_Connection=True;MultipleActiveResultSets=true"
        providerName="System.Data.SqlClient" />
   ```

4. تأكد أن حساب Application Pool لديه صلاحية على قاعدة البيانات.
5. افتح الموقع واعمل تحديث قوي (Ctrl+F5).

> أول تحميل بعد النشر يستغرق ثوانٍ إضافية لترجمة `App_Code` — هذا طبيعي ولمرة واحدة.

## المتطلبات

- Windows Server مع IIS و ASP.NET 4.8
- SQL Server 2019 أو أحدث (مستوى توافق 150)

---

## بنية المستودع

| المجلد | شو فيه |
|---|---|
| الجذر (`App_Code`, `Views`, `Content`, `Scripts`, `bin`) | **الموقع الجاهز للتشغيل** — هاد اللي بينرفع على السيرفر |
| `src/` | **الكود المصدري** لمشروع Visual Studio (Controllers, Models, Data, Helpers) |

### إعادة بناء `bin/itehad.dll`

`App_Code/*.cs` والـ Views بيتترجموا وقت التشغيل، فتعديلهن ما بيحتاج بناء —
بس عدّل ونزّل. أما التعديل على `src/Controllers` أو `src/Models` فلازمه بناء:

```
nuget restore src\itehad.sln
msbuild src\itehad\itehad.csproj /t:Rebuild /p:Configuration=Release
copy src\itehad\bin\itehad.dll bin\itehad.dll
copy src\itehad\bin\itehad.pdb bin\itehad.pdb
```

> `src/packages/` و `src/**/bin/` و `src/**/obj/` مستثنيين من المستودع —
> بيرجعوا لحالهم بـ `nuget restore` وإعادة البناء.

### الملفات المكررة بين الجذر و `src/`

`Content/site.css` و `Scripts/app.js` و `Views/` موجودين بالمكانين. **النسخة
المعتمدة هي اللي بالجذر** (هاي اللي بتشتغل فعلاً)، ونسخة `src/` موجودة عشان
المشروع يفتح ويبني بفيجوال ستوديو. أي تعديل عليهن لازم ينتسخ للمكانين.
