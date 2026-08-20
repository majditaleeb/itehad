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
