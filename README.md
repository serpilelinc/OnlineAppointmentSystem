# 📅 RandevuPlus – Online Randevu Yönetim Sistemi

RandevuPlus; müşterilerin farklı kategorilerde sunulan hizmetleri inceleyebildiği, uygun hizmet veren ve saat seçerek randevu oluşturabildiği; yöneticilerin ise randevuları, hizmetleri, kategorileri ve hizmet verenleri merkezi bir panel üzerinden yönetebildiği web tabanlı bir randevu yönetim sistemidir.

Proje, **ASP.NET Core Web API + ASP.NET Core MVC** mimarisi kullanılarak geliştirilmiştir. Backend ve web arayüzü birbirinden ayrılmış, iletişim HTTP/JSON üzerinden sağlanmıştır.

---

## 🎯 Projenin Amacı

RandevuPlus'ın temel amacı farklı sektörlerde kullanılabilecek merkezi ve genişletilebilir bir randevu altyapısı oluşturmaktır.

Sistem;

- müşteri randevu oluşturma,
- kategori ve hizmet yönetimi,
- hizmet veren yönetimi,
- çalışma saatleri,
- uygunluk kontrolü,
- kullanıcı ve rol yönetimi,
- randevu durum yönetimi

gibi temel süreçleri tek sistem altında toplamaktadır.

---

# 🏗️ Proje Mimarisi

Proje iki ana uygulamadan oluşmaktadır:

```text
OnlineAppointmentSystem
│
├── AppointmentApi
│   └── Backend / REST API
│
└── AppointmentWeb
    └── MVC Web Arayüzü
```

### AppointmentApi

Sistemin backend katmanıdır.

İş kuralları, veritabanı işlemleri, kimlik doğrulama, randevu yönetimi ve diğer temel işlemler burada gerçekleştirilir.

### AppointmentWeb

Kullanıcının gördüğü web uygulamasıdır.

MVC Controller'ları üzerinden AppointmentApi ile iletişim kurar ve API'den alınan verileri Razor View'lar aracılığıyla kullanıcıya gösterir.

---

# 🔄 Sistem Nasıl Çalışıyor?

Temel istek akışı:

```text
Kullanıcı
   │
   ▼
Razor View / JavaScript
   │
   ▼
AppointmentWeb Controller
   │
   │ HTTP / JSON
   ▼
AppointmentApi Controller
   │
   ▼
Service
   │
   ▼
Entity Framework Core
   │
   ▼
SQL Server
```

Örneğin müşteri randevu oluştururken:

```text
Kategori seçilir
      ↓
Hizmet seçilir
      ↓
Hizmet veren seçilir
      ↓
Tarih belirlenir
      ↓
Uygun saatler kontrol edilir
      ↓
Randevu oluşturulur
      ↓
SQL Server'a kaydedilir
```

---

# 🚀 Temel Özellikler

## 👤 Kullanıcı Yönetimi

Sistem farklı kullanıcı rollerini destekleyecek şekilde geliştirilmiştir.

- Admin
- Customer
- Staff

Kullanıcı giriş işlemleri backend üzerinden gerçekleştirilir.

---

## 🔐 JWT Authentication

API güvenliği için **JSON Web Token (JWT)** tabanlı kimlik doğrulama kullanılmıştır.

Başarılı giriş sonrasında oluşturulan token, korumalı API endpointlerine yapılan isteklerde kullanılır.

Rol bazlı yetkilendirme sayesinde kullanıcıların erişebileceği işlemler sınırlandırılabilir.

---

## 📅 Randevu Yönetimi

Sistem üzerinden randevular:

- oluşturulabilir,
- görüntülenebilir,
- güncellenebilir,
- iptal edilebilir,
- silinebilir,
- durumlarına göre takip edilebilir.

Randevular hizmet, hizmet veren, müşteri ve tarih bilgileriyle ilişkilendirilmiştir.

---

## ⏰ Dinamik Uygun Saat Sistemi

Randevu oluşturulurken yalnızca uygun saatlerin gösterilmesi hedeflenmiştir.

Sistem;

- hizmet verenin çalışma saatlerini,
- seçilen tarihi,
- mevcut randevuları,
- hizmet bilgilerini

dikkate alarak uygunluk kontrolü gerçekleştirir.

Bu yapı aynı hizmet verene çakışan randevular oluşturulmasının önlenmesine yardımcı olur.

---

## 👨‍💼 Hizmet Veren Yönetimi

Admin panelinden hizmet verenler yönetilebilir.

Hizmet verenler için:

- personel bilgileri,
- uzmanlık/unvan,
- kullanıcı hesabı,
- sunabileceği hizmetler,
- çalışma günleri,
- çalışma saatleri

tanımlanabilir.

---

## 🛠️ Hizmet Yönetimi

Admin tarafından sistemde sunulan hizmetler yönetilebilir.

Her hizmet için örneğin:

- kategori,
- hizmet adı,
- açıklama,
- süre,
- fiyat

bilgileri tutulabilir.

---

## 📂 Kategori Yönetimi

Hizmetler kategoriler altında organize edilmektedir.

Örnek kategoriler:

- Eğitim
- Ev & Teknik
- Kişisel Bakım
- Otomotiv
- Sağlık
- Temizlik

Bu yapı sayesinde sistem yalnızca tek bir sektöre bağlı kalmadan farklı hizmet alanlarına uyarlanabilir.

---

## 📊 Admin Dashboard

Yönetim paneli üzerinden sistemin genel durumu takip edilebilir.

Dashboard üzerinde örneğin:

- bugünkü randevular,
- bekleyen randevular,
- onaylanan randevular,
- tamamlanan randevular,
- iptal edilen randevular,
- toplam randevu sayısı

görüntülenebilir.

Ayrıca gün içerisindeki randevular hızlı şekilde takip edilebilir.

---

# 💻 Kullanılan Teknolojiler

| Teknoloji | Kullanım Amacı |
|---|---|
| C# | Ana programlama dili |
| .NET / ASP.NET Core | Uygulama platformu |
| ASP.NET Core Web API | Backend REST API |
| ASP.NET Core MVC | Web uygulaması |
| Razor Views | Dinamik web sayfaları |
| HTML5 | Sayfa yapısı |
| CSS3 | Arayüz tasarımı |
| Bootstrap | Responsive UI bileşenleri |
| JavaScript | Dinamik kullanıcı etkileşimleri |
| SQL Server | İlişkisel veritabanı |
| Entity Framework Core | ORM ve veritabanı erişimi |
| LINQ | Veri sorgulama |
| JWT | Authentication / Authorization |
| AutoMapper | Model ve DTO dönüşümleri |
| Swagger / OpenAPI | API geliştirme ve test |
| Git | Versiyon kontrolü |
| GitHub | Kaynak kod yönetimi |

---

# 🧩 Kullanılan Backend Yaklaşımları

Projede kodun daha sürdürülebilir ve anlaşılır olması amacıyla katmanlı sorumluluk yaklaşımı uygulanmıştır.

### Controller

HTTP isteklerini karşılar ve ilgili işlemi Service katmanına yönlendirir.

### Service

Sistemin temel iş kurallarını içerir.

Örneğin:

- randevu uygunluk kontrolü,
- hizmet veren işlemleri,
- randevu işlemleri,
- kullanıcı işlemleri.

### DTO

API'ye giren ve API'den çıkan verilerin kontrollü şekilde taşınmasını sağlar.

Veritabanı entity'lerinin doğrudan dış dünyaya açılmasını azaltır.

### Model / Entity

SQL Server'da saklanan verilerin C# tarafındaki karşılığını temsil eder.

### DbContext

Entity Framework Core üzerinden C# modelleri ile SQL Server arasındaki bağlantıyı yönetir.

---

# 🔗 Entity İlişkileri

Sistem içerisinde temel olarak aşağıdaki yapılar birbiriyle ilişkilidir:

```text
User
 │
 ├── Customer
 │
 └── Staff
       │
       ├── WorkingHours
       │
       └── ServiceTypes
              │
              ▼
           Services
              │
              ▼
          Categories


Customer
   │
   └──────────────┐
                  ▼
             Appointment
                  ▲
                  │
Staff ────────────┤
                  │
Service ──────────┘
```

Bu ilişkiler Entity Framework Core üzerinden yönetilmektedir.

---

# 🌐 Dinamik Randevu Oluşturma

Randevu oluşturma ekranında JavaScript kullanılarak kullanıcı deneyimi daha dinamik hale getirilmiştir.

Örneğin:

```text
Kategori değişti
      ↓
İlgili hizmetler yüklenir
      ↓
Hizmet değişti
      ↓
Fiyat ve hizmet bilgileri güncellenir
      ↓
Hizmet verenler yüklenir
      ↓
Tarih seçilir
      ↓
Uygun saatler getirilir
```

Böylece kullanıcı seçim yaptıkça gerekli alanlar dinamik olarak güncellenir.

---

# 🔒 Güvenlik

Projede güvenlik açısından aşağıdaki yaklaşımlar kullanılmıştır:

- JWT tabanlı authentication
- Rol bazlı authorization
- Şifrelerin hashlenerek saklanması
- DTO kullanımı
- ASP.NET Core model validation
- Anti-forgery token kullanımı
- API endpoint erişim kontrolleri
- Merkezi hata yönetimi

> Gerçek ortamda JWT secret, SMTP şifreleri ve diğer hassas bilgiler kaynak kod içerisinde tutulmamalıdır. Environment Variables, User Secrets veya uygun bir secret-management sistemi kullanılmalıdır.

---

# ⚠️ Hata Yönetimi

API ve Web katmanlarında hata yönetiminin merkezi şekilde ele alınabilmesi için middleware yaklaşımı kullanılmaktadır.

Bu sayede uygulama genelinde oluşabilecek hataların kontrollü şekilde yönetilmesi ve kullanıcıya daha anlaşılır cevaplar verilmesi amaçlanmıştır.

---

# 🗄️ Veritabanı

Projede **Microsoft SQL Server** kullanılmaktadır.

Entity Framework Core sayesinde veritabanı işlemleri C# üzerinden gerçekleştirilmektedir.

Migration sistemi ile veritabanı şemasındaki değişiklikler versiyonlanabilir.

Temel yaklaşım:

```text
C# Model
    ↓
Entity Framework Core
    ↓
Migration
    ↓
SQL Server
```

---

# 🧪 API Testleri

Development ortamında API endpointlerini görüntülemek ve test etmek için **Swagger / OpenAPI** kullanılabilir.

Swagger üzerinden:

- GET
- POST
- PUT
- DELETE

endpointleri incelenebilir ve API istekleri test edilebilir.

---

# 📁 Genel Proje Yapısı

```text
OnlineAppointmentSystem
│
├── AppointmentApi
│   ├── Controllers
│   ├── Data
│   ├── DTOs
│   ├── Exceptions
│   ├── Middleware
│   ├── Migrations
│   ├── Models
│   ├── Services
│   ├── Program.cs
│   └── appsettings.json
│
├── AppointmentWeb
│   ├── Controllers
│   ├── Middleware
│   ├── Models
│   ├── Services
│   ├── Views
│   ├── wwwroot
│   ├── Program.cs
│   └── appsettings.json
│
├── .gitignore
└── README.md
```

---

# ⚙️ Projeyi Çalıştırma

## Gereksinimler

Projeyi çalıştırabilmek için aşağıdaki araçların kurulu olması gerekir:

- .NET SDK
- SQL Server
- Git
- Visual Studio / Visual Studio Code veya uyumlu bir IDE

---

## 1. Repository'yi Klonlayın

```bash
git clone <repository-url>
```

Proje klasörüne geçin:

```bash
cd OnlineAppointmentSystem
```

---

## 2. Veritabanını Hazırlayın

`AppointmentApi/appsettings.json` içerisindeki connection string kendi SQL Server ortamınıza göre yapılandırılmalıdır.

Ardından API projesinde migration'lar uygulanabilir:

```bash
cd AppointmentApi
dotnet ef database update
```

---

## 3. API'yi Çalıştırın

```bash
cd AppointmentApi
dotnet run
```

API çalıştıktan sonra development ortamında Swagger/OpenAPI arayüzü üzerinden endpointler incelenebilir.

---

## 4. Web Uygulamasını Çalıştırın

Yeni bir terminal açın:

```bash
cd AppointmentWeb
dotnet run
```

Web uygulaması AppointmentApi ile HTTP üzerinden iletişim kuracaktır.

> API adresi değişirse AppointmentWeb içerisindeki API BaseAddress yapılandırmasının da güncellenmesi gerekir.

---

# 📈 Gelecekte Eklenebilecek Özellikler

Proje mimarisi gelecekte aşağıdaki özelliklerin eklenmesine uygun şekilde genişletilebilir:

- E-posta bildirimleri
- SMS bildirimleri
- Şifre sıfırlama
- Gelişmiş kullanıcı profil yönetimi
- Randevu hatırlatmaları
- Takvim görünümü
- Raporlama ve istatistik ekranları
- Gelişmiş dashboard
- Audit logging
- Refresh Token yapısı
- Rate Limiting
- Docker desteği
- CI/CD pipeline
- Production deployment

---

# 📌 Proje Durumu

RandevuPlus şu anda temel randevu yönetimi süreçlerini destekleyen çalışan bir web uygulaması olarak geliştirilmiştir.

Proje kapsamında özellikle:

**Web UI → MVC → REST API → Service → Entity Framework Core → SQL Server**

akışının uygulanması ve gerçek bir web uygulamasındaki frontend/backend/veritabanı iletişiminin kurulması hedeflenmiştir.

---

## 👩‍💻 Geliştirici

**Serpil Elinç**

Bilgisayar Mühendisliği  
Yazılım Geliştirme Staj Projesi

---

## 📄 Lisans

Bu proje eğitim ve staj çalışması kapsamında geliştirilmiştir.
