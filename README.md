# ETMP Player - Project Documentation

## Project Overview
ETMP Player is a modern, high-performance desktop music player developed using the **.NET 9** framework and **WPF** (Windows Presentation Foundation).

## Technical Architecture

### Core Framework
- **Platform**: Windows Desktop (x64)
- **Framework**: .NET 9.0
- **UI System**: WPF (Windows Presentation Foundation)
- **Design Pattern**: MVVM (Model-View-ViewModel) with Dependency Injection (DI).

### Data Management
- **Database**: SQLite (via Entity Framework Core).
- **ORM**: Entity Framework Core 7.0.
- **Data Persistence**: 
  - Application settings and library metadata are stored in a local SQLite database (`data/player`).
  - Playlists are managed via a robust database-backed system.
- **Sandbox System**: The application implements a file sandboxing mechanism where songs added to playlists are automatically copied to a local `songs/` directory to strictly prevent broken file links.

### Audio Engine
- **Audio Decoding**: Uses `NAudio` for standard audio formats (MP3, WAV, FLAC).
- **MIDI Support**: Integrated `Melanchall.DryWetMidi` for native MIDI file playback with channel-based volume control and instrument detection.
- **Playback Engine**: 
  - `WaveOut` for low-latency audio output.
  - Custom `MidiPlayerService` using the system's default General MIDI synthesizer.

### Visualization Engine
- **Hybrid Rendering**: 
  - **C# Layer**: Calculates FFT (Fast Fourier Transform) data using `NAudio`.
  - **Native Layer**: Uses a custom C++ DLL (`VisualizerNative.dll`) which renders the visualization using **DirectX 11**.
- **Interop**: `DirectXVisualizerHost` manages the P/Invoke calls between the managed WPF application and the unmanaged C++ rendering engine.

## Feature Set

### 1. Music Library
- **Import**: Recursive folder scanning to import entire music collections.
- **Metadata**: Automatic extraction of ID3 tags (Title, Artist, Album, Duration, Cover Art) using `TagLibSharp`.
- **Search**: Real-time filtering of the library by song title, artist, or album.

### 2. Audio Playback
- **Formats**: Support for MP3, WAV, FLAC, M4A, WMA, MID, MIDI.
- **Controls**: Standard playback controls (Play, Pause, Next, Previous), Shuffle, and Repeat modes (One, All).
- **Equalizer**: 10-Band Parametric Equalizer with:
  - Adjustable Preamp
  - Preset System (Flat, etc.)
  - Smooth animation interpolation between setting changes.

### 3. MIDI Player
- **Channel Mixer**: Independent volume control for all 16 MIDI channels.
- **Instrumentation**: Real-time display of instruments used in each channel (e.g., "Acoustic Grand Piano", "Violin").
- **Solo/Mute**: Infrastructure supports soloing and muting individual channels.

### 4. Playlist Management
- **Database Storage**: Playlists are securely stored in SQLite.
- **File Integrity**: Automatic backup of song files ensures playlists remain playable even if original files are moved or deleted.
- **UI**: Drag-and-drop support for adding files directly to playlists.

### 5. Customization
- **Themes**: Support for UI theming (colors, fonts).
- **Settings**: Configurable home screen typography.

## Project Structure

- **`ETMPClient`**: The main WPF application project.
  - **`ViewModels`**: Contains the application logic (MVVM).
  - **`Views`**: XAML files defining the user interface.
  - **`Services`**: Core services (Music Player, MIDI Player, Navigation, Localization).
  - **`Stores`**: State management for the application.
  - **`Controls`**: Custom UI controls, including the DirectX host.
  
- **`ETMPData`**: Data access layer.
  - **`DataEntities`**: POCO classes representing database tables.
  - **`Migrations`**: Entity Framework database migration files.


## Dependencies

- **`LibVLCSharp.WPF`**: Cross-platform multimedia framework (Note: Included but primary playback uses NAudio).
- **`Melanchall.DryWetMidi`**: For reading and manipulating MIDI files.
- **`NAudio`**: For audio playback and FFT analysis.
- **`Microsoft.EntityFrameworkCore.Sqlite`**: For database operations.
- **`TagLibSharp`**: For reading audio file metadata.
- **`Microsoft.Extensions.DependencyInjection`**: For Inversion of Control (IoC).
