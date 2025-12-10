# ردیاب قیمت طلا و دلار

یک وب‌سایت ASP.NET Core برای نمایش قیمت طلا و دلار

## ویژگی‌ها

- نمایش قیمت طلا و دلار
- نمایش تغییرات قیمت (مثبت/منفی)
- نمایش درصد تغییرات
- بروزرسانی خودکار هر 30 ثانیه
- رابط کاربری زیبا و واکنش‌گرا

## نحوه اجرا

1. نصب .NET 7 SDK
2. اجرای دستورات زیر:

```bash
dotnet restore
dotnet run
```

3. باز کردن مرورگر و رفتن به آدرس: `https://localhost:5001` یا `http://localhost:5000`

## توضیحات

- پروژه از API واقعی برای دریافت قیمت‌های لحظه‌ای استفاده می‌کند
- API پیش‌فرض: `nerkh.io` (API ایرانی)
- در صورت خطا در API اصلی، از API های جایگزین استفاده می‌شود
- در صورت خطا در همه API ها، داده‌های نمونه نمایش داده می‌شود

## تنظیمات API

می‌توانید URL های API را در فایل `appsettings.json` تغییر دهید:

```json
{
  "ApiSettings": {
    "GoldApiUrl": "https://api.nerkh.io/v1/gold",
    "DollarApiUrl": "https://api.nerkh.io/v1/usd",
    "TimeoutSeconds": 10
  }
}
```

## API های استفاده شده

### API اصلی:
- **nerkh.io**: API ایرانی برای قیمت طلا و دلار
  - طلا: `https://api.nerkh.io/v1/gold`
  - دلار: `https://api.nerkh.io/v1/usd`

### API های جایگزین:
- **metals.live**: برای قیمت طلا (بین‌المللی)
- **exchangerate-api.com**: برای نرخ ارز (بین‌المللی)

## ساختار پاسخ API

API باید پاسخ JSON با ساختار زیر را برگرداند:

```json
{
  "price": 2500000,
  "change": 50000,
  "changePercent": 2.04,
  "timestamp": 1234567890
}
```

