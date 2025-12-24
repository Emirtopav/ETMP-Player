# Sürüm 2.0 Sonrası Özellikler ve Teknik Detaylar

## � Ses Motoru ve Format Destekleri

### 1. Yerel MIDI Entegrasyonu (`MidiPlayerService`)
- **Kütüphane**: `Melanchall.DryWetMidi` kütüphanesi entegre edildi.
- **Kanal Mikseri**: 16 MIDI kanalının her biri için bağımsız **Ses Seviyesi (Volume)** kontrolü UI üzerinden sağlanmaktadır.
- **Enstrüman Algılama**: MIDI dosyasındaki `ProgramChange` olayları taranarak, her kanalda kullanılan enstrüman (örn. "Acoustic Grand Piano") kullanıcı arayüzünde gösterilir.
- **Sıfır Gecikme**: Sistem synth'i (Microsoft GS Wavetable Synth) kullanılarak düşük gecikmeli oynatma sağlanır.
- *Not: MIDI Mute/Solo mantığı arka planda (`ChannelControl` sınıfında) hazırdır ancak mevcut arayüzde buton bağlantıları henüz aktif değildir.*

### 2. Gelişmiş Ekolayzer (`EqualizerViewModel`)
- **Teknoloji**: `NAudio` tabanlı 10 bantlı parametrik ekolayzer.
- **Frekans Bantları**: 31Hz, 62Hz, 125Hz, 250Hz, 500Hz, 1kHz, 2kHz, 4kHz, 8kHz, 16kHz.
- **Animasyonlu UI**: Ekolayzer ayarları değiştirildiğinde veya hazır ayar seçildiğinde geçişler animasyonlu (interpolation) olarak yapılır.
- **Hazır Ayarlar (Presets)**: "Flat" ve diğer profiller arası geçiş desteği.
- **Otomatik Kayıt**: Yapılan her değişiklik `EqualizerSettingsManager` aracılığıyla anlık olarak json dosyasına kaydedilir.


## 📂 Dosya ve Çalma Listesi Yönetimi

### 3. Güvenli Çalma Listesi Mimarisi (`PlaylistStore`)
- **Veritabanı Entegrasyonu**: Çalma listeleri güvenilir SQLite veritabanında saklanır.
- **Otomatik Dosya Yedekleme**: Bir şarkı çalma listesine eklendiğinde, kaynak dosya otomatik olarak uygulamanın yerel `songs/` klasörüne kopyalanır (`File.Copy`). Bu, şarkıların orijinal yerleri değişse bile listenin bozulmamasını sağlar (Sandbox mantığı).
- **Sürükle-Bırak**: `IFilesDropAsync` arayüzü ile dışarıdan dosya sürükleyerek listeye ekleme desteği.

### 4. Single-File (Tek Dosya) Mimarisi
- **Standalone Publish**: Proje `.csproj` ayarları ve yayınlama profilleri, uygulamanın `.NET Runtime` gerektirmeden tek bir `.exe` (Self-Contained) olarak çalışmasını sağlayacak şekilde yapılandırılmıştır.

## ⚙️ Arayüz ve Dil

### 5. Altyapısal Dil Desteği (`LocalizationService`)
- **JSON Kaynaklar**: Diller `Resources/Languages/` altında JSON formatında tutulur (en-US, tr-TR).
- **Dinamik Yükleme**: Uygulama çalışırken dil değiştirme altyapısı mevcuttur.

## 🛠 Kaldırılan/Devre Dışı Bırakılanlar
- **Youtube Entegrasyonu**: Stabilite sorunları nedeniyle projeden tamamen temizlenmiştir.
- **Video Oynatıcı**: Odak noktası müzik olduğu için video oynatma özellikleri çıkarılmıştır.
