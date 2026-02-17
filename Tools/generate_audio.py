#!/usr/bin/env python3
"""
Generate procedural audio assets (WAV files) for the game.
Creates music tracks, sound effects, and ambient sounds.
Uses basic wave synthesis for Indian-style music patterns.
"""
import struct
import math
import os
import random

MUSIC_DIR = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project/Audio/Music"
SFX_DIR = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project/Audio/SFX"
RESOURCES_DIR = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project/Resources/Audio"

for d in [MUSIC_DIR, SFX_DIR, RESOURCES_DIR, os.path.join(RESOURCES_DIR, "Music"), os.path.join(RESOURCES_DIR, "SFX")]:
    os.makedirs(d, exist_ok=True)

SAMPLE_RATE = 44100
MAX_AMP = 32767


def write_wav(filename, samples, sample_rate=SAMPLE_RATE, channels=1):
    """Write samples to WAV file."""
    num_samples = len(samples)
    data_size = num_samples * 2 * channels

    with open(filename, 'wb') as f:
        # RIFF header
        f.write(b'RIFF')
        f.write(struct.pack('<I', 36 + data_size))
        f.write(b'WAVE')

        # fmt chunk
        f.write(b'fmt ')
        f.write(struct.pack('<I', 16))  # chunk size
        f.write(struct.pack('<H', 1))   # PCM
        f.write(struct.pack('<H', channels))
        f.write(struct.pack('<I', sample_rate))
        f.write(struct.pack('<I', sample_rate * channels * 2))  # byte rate
        f.write(struct.pack('<H', channels * 2))  # block align
        f.write(struct.pack('<H', 16))  # bits per sample

        # data chunk
        f.write(b'data')
        f.write(struct.pack('<I', data_size))
        for s in samples:
            value = max(-MAX_AMP, min(MAX_AMP, int(s * MAX_AMP)))
            f.write(struct.pack('<h', value))


def sine_wave(freq, duration, amplitude=0.5, sample_rate=SAMPLE_RATE):
    """Generate sine wave samples."""
    samples = []
    for i in range(int(sample_rate * duration)):
        t = i / sample_rate
        samples.append(amplitude * math.sin(2 * math.pi * freq * t))
    return samples


def triangle_wave(freq, duration, amplitude=0.5, sample_rate=SAMPLE_RATE):
    """Generate triangle wave (warmer sound, good for flute-like tones)."""
    samples = []
    for i in range(int(sample_rate * duration)):
        t = i / sample_rate
        period = 1 / freq
        phase = (t % period) / period
        if phase < 0.5:
            value = 4 * phase - 1
        else:
            value = 3 - 4 * phase
        samples.append(amplitude * value)
    return samples


def sawtooth_wave(freq, duration, amplitude=0.3, sample_rate=SAMPLE_RATE):
    """Generate sawtooth wave (good for string-like tones)."""
    samples = []
    for i in range(int(sample_rate * duration)):
        t = i / sample_rate
        period = 1 / freq
        phase = (t % period) / period
        samples.append(amplitude * (2 * phase - 1))
    return samples


def noise(duration, amplitude=0.3, sample_rate=SAMPLE_RATE):
    """Generate white noise."""
    samples = []
    for i in range(int(sample_rate * duration)):
        samples.append(amplitude * (random.random() * 2 - 1))
    return samples


def envelope(samples, attack=0.05, decay=0.1, sustain=0.7, release=0.2):
    """Apply ADSR envelope to samples."""
    total = len(samples)
    attack_samples = int(total * attack)
    decay_samples = int(total * decay)
    release_samples = int(total * release)
    sustain_samples = total - attack_samples - decay_samples - release_samples

    result = []
    for i, s in enumerate(samples):
        if i < attack_samples:
            amp = i / max(1, attack_samples)
        elif i < attack_samples + decay_samples:
            di = i - attack_samples
            amp = 1.0 - (1.0 - sustain) * (di / max(1, decay_samples))
        elif i < attack_samples + decay_samples + sustain_samples:
            amp = sustain
        else:
            ri = i - (attack_samples + decay_samples + sustain_samples)
            amp = sustain * (1 - ri / max(1, release_samples))
        result.append(s * max(0, amp))
    return result


def mix(tracks):
    """Mix multiple tracks together."""
    max_len = max(len(t) for t in tracks)
    result = [0.0] * max_len
    for track in tracks:
        for i, s in enumerate(track):
            result[i] += s
    # Normalize
    peak = max(abs(s) for s in result) if result else 1
    if peak > 0:
        result = [s / peak * 0.8 for s in result]
    return result


def add_reverb(samples, delay_ms=80, decay=0.3, sample_rate=SAMPLE_RATE):
    """Simple reverb effect."""
    delay_samples = int(sample_rate * delay_ms / 1000)
    result = list(samples)
    for i in range(delay_samples, len(result)):
        result[i] += result[i - delay_samples] * decay
    # Normalize
    peak = max(abs(s) for s in result) if result else 1
    if peak > 0:
        result = [s / peak * 0.8 for s in result]
    return result


# Indian musical scale (Raga Bilawal - similar to major scale)
RAGA_BILAWAL = [261.63, 293.66, 329.63, 349.23, 392.00, 440.00, 493.88, 523.25]  # C major
# Raga Yaman (evening raga, beautiful)
RAGA_YAMAN = [261.63, 293.66, 329.63, 370.00, 392.00, 440.00, 493.88, 523.25]  # C with #4
# Raga Bhairavi (morning, melancholic)
RAGA_BHAIRAVI = [261.63, 277.18, 311.13, 349.23, 392.00, 415.30, 466.16, 523.25]  # flats


def generate_melody(raga, pattern, note_duration=0.3, wave_func=triangle_wave, amp=0.4):
    """Generate a melody from a raga and pattern."""
    melody = []
    for note_idx in pattern:
        if note_idx < 0:  # Rest
            melody.extend([0.0] * int(SAMPLE_RATE * note_duration))
        else:
            freq = raga[note_idx % len(raga)]
            # Octave shift
            if note_idx >= len(raga):
                freq *= 2
            note = envelope(wave_func(freq, note_duration, amp))
            melody.extend(note)
    return melody


def generate_drone(freq, duration, amplitude=0.15):
    """Generate tanpura-like drone."""
    # Combine fundamental, fifth, and octave
    fundamental = sine_wave(freq, duration, amplitude)
    fifth = sine_wave(freq * 3/2, duration, amplitude * 0.4)
    octave = sine_wave(freq * 2, duration, amplitude * 0.3)

    drone = mix([fundamental, fifth, octave])
    return drone


def generate_tabla_hit(pitch='high', duration=0.15):
    """Generate tabla-like percussion hit."""
    if pitch == 'high':
        # Dha/Na - sharp attack
        freq = 400
        s = envelope(sine_wave(freq, duration, 0.6), attack=0.001, decay=0.05, sustain=0.1, release=0.6)
        n = envelope(noise(duration, 0.2), attack=0.001, decay=0.02, sustain=0.0, release=0.3)
        return mix([s, n])
    else:
        # Bass hit
        freq = 120
        s = envelope(sine_wave(freq, duration * 1.5, 0.7), attack=0.001, decay=0.1, sustain=0.2, release=0.5)
        n = envelope(noise(duration, 0.15), attack=0.001, decay=0.01, sustain=0.0, release=0.2)
        return mix([s, n])


# ============ MUSIC TRACKS ============

def generate_act1_music():
    """Act I: Happy Home - warm, golden, traditional Assamese feel."""
    duration = 30  # 30 second loop

    # Tanpura drone in C
    drone = generate_drone(130.81, duration, 0.12)

    # Melody using Raga Bilawal (happy, open)
    pattern = [0, 2, 4, 5, 4, 2, 0, -1,
               2, 4, 5, 7, 5, 4, 2, -1,
               4, 5, 7, 8, 7, 5, 4, 2,
               0, 2, 0, -1, -1, -1, -1, -1] * 4
    melody = generate_melody(RAGA_BILAWAL, pattern, 0.25, triangle_wave, 0.35)

    # Pad melody to drone length
    while len(melody) < len(drone):
        melody.extend(melody[:len(drone) - len(melody)])
    melody = melody[:len(drone)]

    # Gentle rhythm (tabla pattern)
    rhythm = []
    beat_duration = 60 / 90  # 90 BPM
    beat_samples = int(SAMPLE_RATE * beat_duration)
    while len(rhythm) < len(drone):
        hit = generate_tabla_hit('high', 0.1)
        rhythm.extend(hit)
        rhythm.extend([0.0] * max(0, beat_samples - len(hit)))
    rhythm = rhythm[:len(drone)]

    result = mix([drone, melody, rhythm])
    result = add_reverb(result, 100, 0.2)
    return result


def generate_act2_music():
    """Act II: Descent - darker, more tension, descending patterns."""
    duration = 30

    drone = generate_drone(110.00, duration, 0.10)  # Lower drone

    # Raga Bhairavi - melancholic
    pattern = [7, 6, 5, 4, 3, 2, 1, 0,  # Descending
               0, 1, 2, 3, 2, 1, 0, -1,
               3, 4, 5, 4, 3, 2, 1, 0,
               0, -1, 0, -1, -1, -1, -1, -1] * 4
    melody = generate_melody(RAGA_BHAIRAVI, pattern, 0.28, sawtooth_wave, 0.25)

    while len(melody) < len(drone):
        melody.extend(melody[:len(drone) - len(melody)])
    melody = melody[:len(drone)]

    # Heavier rhythm
    rhythm = []
    beat_duration = 60 / 100
    beat_samples = int(SAMPLE_RATE * beat_duration)
    i = 0
    while len(rhythm) < len(drone):
        pitch = 'low' if i % 4 == 0 else 'high'
        hit = generate_tabla_hit(pitch, 0.12)
        rhythm.extend(hit)
        rhythm.extend([0.0] * max(0, beat_samples - len(hit)))
        i += 1
    rhythm = rhythm[:len(drone)]

    result = mix([drone, melody, rhythm])
    result = add_reverb(result, 150, 0.3)
    return result


def generate_dheki_rhythm():
    """Dheki rhythm track - for the rice husker minigame."""
    duration = 60  # Longer for gameplay

    # Start at 90 BPM, this track provides the beat reference
    samples = []
    bpm = 90
    beat_num = 0

    while len(samples) < SAMPLE_RATE * duration:
        beat_interval = 60 / bpm

        # Dheki thump (heavy, wooden)
        if beat_num % 2 == 0:
            hit = generate_tabla_hit('low', 0.2)
        else:
            hit = generate_tabla_hit('high', 0.15)

        samples.extend(hit)
        remaining = int(SAMPLE_RATE * beat_interval) - len(hit)
        if remaining > 0:
            samples.extend([0.0] * remaining)

        # Gradually increase BPM
        bpm = min(150, 90 + beat_num * 0.3)
        beat_num += 1

    samples = samples[:SAMPLE_RATE * duration]
    return samples


def generate_act3_music():
    """Act III: Spirit Awakens - ethereal, mysterious, split between past and present."""
    duration = 30

    # Higher, ethereal drone
    drone = generate_drone(196.00, duration, 0.08)  # G

    # Raga Yaman - mystical evening feel
    pattern = [0, 3, 4, 5, 7, 5, 4, 3,
               4, 5, 7, 8, 7, 5, 4, -1,
               -1, 0, 2, 3, 4, 3, 2, 0,
               -1, -1, -1, 0, -1, -1, -1, -1] * 4
    melody = generate_melody(RAGA_YAMAN, pattern, 0.35, sine_wave, 0.3)

    while len(melody) < len(drone):
        melody.extend(melody[:len(drone) - len(melody)])
    melody = melody[:len(drone)]

    result = mix([drone, melody])
    result = add_reverb(result, 200, 0.4)
    return result


def generate_act4_boss_music():
    """Act IV: Boss fight - intense, driving, dramatic."""
    duration = 30

    # Intense low drone
    drone = generate_drone(98.00, duration, 0.15)  # Low G

    # Fast, aggressive pattern
    pattern = [0, 0, 3, 5, 0, 0, 3, 5,
               7, 5, 3, 0, 7, 5, 3, 0,
               0, 2, 3, 5, 7, 5, 3, 2,
               0, 0, -1, 0, 0, -1, 0, 0] * 4
    melody = generate_melody(RAGA_BHAIRAVI, pattern, 0.15, sawtooth_wave, 0.35)

    while len(melody) < len(drone):
        melody.extend(melody[:len(drone) - len(melody)])
    melody = melody[:len(drone)]

    # Intense rhythm
    rhythm = []
    beat_duration = 60 / 140  # Fast BPM
    beat_samples = int(SAMPLE_RATE * beat_duration)
    i = 0
    while len(rhythm) < len(drone):
        hit = generate_tabla_hit('low' if i % 2 == 0 else 'high', 0.08)
        rhythm.extend(hit)
        rhythm.extend([0.0] * max(0, beat_samples - len(hit)))
        i += 1
    rhythm = rhythm[:len(drone)]

    result = mix([drone, melody, rhythm])
    result = add_reverb(result, 60, 0.15)
    return result


def generate_epilogue_music():
    """Epilogue - peaceful, hopeful, dawn breaking."""
    duration = 30

    # Warm drone
    drone = generate_drone(196.00, duration, 0.10)

    # Ascending, hopeful melody
    pattern = [0, 2, 4, 5, 7, 8, 7, 5,
               4, 5, 7, 8, 10, 8, 7, -1,
               8, 7, 5, 4, 5, 7, 8, 10,
               8, 7, 5, 4, 2, 0, -1, -1] * 4
    melody = generate_melody(RAGA_BILAWAL, pattern, 0.35, triangle_wave, 0.3)

    while len(melody) < len(drone):
        melody.extend(melody[:len(drone) - len(melody)])
    melody = melody[:len(drone)]

    result = mix([drone, melody])
    result = add_reverb(result, 250, 0.4)
    return result


def generate_menu_music():
    """Menu theme - gentle nahor tree theme."""
    duration = 20

    drone = generate_drone(164.81, duration, 0.08)

    pattern = [0, -1, 4, -1, 5, 4, 2, 0,
               -1, -1, 2, -1, 4, 2, 0, -1] * 5
    melody = generate_melody(RAGA_YAMAN, pattern, 0.4, triangle_wave, 0.25)

    while len(melody) < len(drone):
        melody.extend(melody[:len(drone) - len(melody)])
    melody = melody[:len(drone)]

    result = mix([drone, melody])
    result = add_reverb(result, 300, 0.35)
    return result


# ============ SFX ============

def generate_sfx():
    """Generate all sound effects."""
    sfx = {}

    # Footsteps
    for surface in ['wood', 'grass', 'stone']:
        for i in range(3):
            if surface == 'wood':
                freq = 200 + random.randint(-30, 30)
                s = envelope(mix([sine_wave(freq, 0.05, 0.5), noise(0.05, 0.3)]),
                           attack=0.001, decay=0.01, sustain=0.0, release=0.5)
            elif surface == 'grass':
                s = envelope(noise(0.08, 0.2), attack=0.001, decay=0.02, sustain=0.0, release=0.6)
            else:
                freq = 300 + random.randint(-50, 50)
                s = envelope(mix([sine_wave(freq, 0.04, 0.4), noise(0.04, 0.2)]),
                           attack=0.001, decay=0.005, sustain=0.0, release=0.4)
            sfx[f'footstep_{surface}_{i}'] = s

    # Spirit pulse whoosh
    whoosh = []
    for i in range(int(SAMPLE_RATE * 0.8)):
        t = i / SAMPLE_RATE
        freq = 200 + 800 * t
        amp = 0.4 * math.sin(math.pi * t / 0.8)
        whoosh.append(amp * math.sin(2 * math.pi * freq * t) + 0.1 * (random.random() * 2 - 1) * amp)
    sfx['spirit_pulse'] = whoosh

    # Drum hit
    sfx['drum_hit'] = generate_tabla_hit('low', 0.3)
    sfx['drum_tap'] = generate_tabla_hit('high', 0.2)

    # Heartbeat
    beat1 = envelope(sine_wave(60, 0.15, 0.6), attack=0.01, decay=0.05, sustain=0.1, release=0.5)
    gap = [0.0] * int(SAMPLE_RATE * 0.1)
    beat2 = envelope(sine_wave(50, 0.12, 0.4), attack=0.01, decay=0.05, sustain=0.05, release=0.4)
    sfx['heartbeat'] = beat1 + gap + beat2

    # UI click
    sfx['ui_click'] = envelope(sine_wave(800, 0.05, 0.3), attack=0.001, decay=0.01, sustain=0.0, release=0.3)

    # Menu hover
    sfx['ui_hover'] = envelope(sine_wave(600, 0.03, 0.2), attack=0.001, decay=0.005, sustain=0.0, release=0.3)

    # Memory flash
    flash = []
    for i in range(int(SAMPLE_RATE * 0.5)):
        t = i / SAMPLE_RATE
        freq = 1000 - 600 * t
        amp = 0.3 * (1 - t / 0.5)
        flash.append(amp * math.sin(2 * math.pi * freq * t))
    sfx['memory_flash'] = add_reverb(flash, 150, 0.4)

    # Catch/detection alert
    alert = envelope(sine_wave(880, 0.3, 0.4), attack=0.001, decay=0.05, sustain=0.3, release=0.3)
    alert2 = envelope(sine_wave(1100, 0.15, 0.3), attack=0.001, decay=0.03, sustain=0.1, release=0.3)
    sfx['alert'] = mix([alert, [0.0] * int(SAMPLE_RATE * 0.15) + alert2])

    # Wind ambient
    wind = []
    for i in range(int(SAMPLE_RATE * 5)):
        t = i / SAMPLE_RATE
        amp = 0.1 * (0.5 + 0.5 * math.sin(t * 0.3))
        wind.append(amp * (random.random() * 2 - 1))
    sfx['wind_ambient'] = wind

    # Water/river ambient
    water = []
    for i in range(int(SAMPLE_RATE * 5)):
        t = i / SAMPLE_RATE
        amp = 0.08 * (0.6 + 0.4 * math.sin(t * 0.5 + random.random()))
        water.append(amp * (random.random() * 2 - 1))
    sfx['water_ambient'] = water

    # Door open/close
    sfx['door_open'] = envelope(
        mix([noise(0.3, 0.2), sine_wave(150, 0.3, 0.3)]),
        attack=0.01, decay=0.1, sustain=0.2, release=0.4
    )

    # Item collect
    collect = envelope(sine_wave(523, 0.1, 0.3), attack=0.001, decay=0.02, sustain=0.1, release=0.3)
    collect2 = envelope(sine_wave(659, 0.1, 0.3), attack=0.001, decay=0.02, sustain=0.1, release=0.3)
    collect3 = envelope(sine_wave(784, 0.15, 0.3), attack=0.001, decay=0.02, sustain=0.1, release=0.4)
    sfx['item_collect'] = collect + collect2 + collect3

    # Boss hit
    sfx['boss_hit'] = envelope(
        mix([sine_wave(80, 0.4, 0.5), noise(0.4, 0.3), sine_wave(160, 0.4, 0.3)]),
        attack=0.001, decay=0.05, sustain=0.2, release=0.5
    )

    # Victory/triumph
    tri = []
    for note_freq in [392, 494, 587, 784]:
        tri.extend(envelope(triangle_wave(note_freq, 0.3, 0.4), attack=0.01, decay=0.05, sustain=0.4, release=0.3))
    sfx['victory'] = add_reverb(tri, 200, 0.3)

    return sfx


if __name__ == "__main__":
    print("Generating audio assets...")
    print("Using procedural synthesis (Indian raga-based music)")
    print()

    # Music tracks
    print("--- MUSIC ---")
    tracks = {
        'menu': generate_menu_music,
        'act1_happy': generate_act1_music,
        'act1_funeral': generate_act2_music,  # Reuse darker variant
        'act2_descent': generate_act2_music,
        'act2_dheki': generate_dheki_rhythm,
        'act2_burial': generate_act2_music,
        'act3_arrival': generate_act3_music,
        'act3_dual': generate_act3_music,
        'act4_boss': generate_act4_boss_music,
        'epilogue': generate_epilogue_music,
    }

    for name, generator in tracks.items():
        print(f"Generating {name}...", end=' ')
        samples = generator()
        # Save to both Music dir and Resources dir
        write_wav(os.path.join(MUSIC_DIR, f"{name}.wav"), samples)
        write_wav(os.path.join(RESOURCES_DIR, "Music", f"{name}.wav"), samples)
        print(f"OK ({len(samples)/SAMPLE_RATE:.1f}s)")

    # SFX
    print("\n--- SOUND EFFECTS ---")
    sfx = generate_sfx()
    for name, samples in sfx.items():
        write_wav(os.path.join(SFX_DIR, f"{name}.wav"), samples)
        write_wav(os.path.join(RESOURCES_DIR, "SFX", f"{name}.wav"), samples)
        print(f"Generated: {name}.wav ({len(samples)/SAMPLE_RATE:.2f}s)")

    print(f"\nTotal music tracks: {len(tracks)}")
    print(f"Total SFX: {len(sfx)}")
    print("All audio assets generated!")
