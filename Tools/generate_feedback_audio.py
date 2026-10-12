"""Generate original, short mono feedback clips using only Python's standard library."""
import math
from pathlib import Path
import random
import struct
import wave

RATE = 22050
OUTPUT = Path(__file__).resolve().parents[1] / "Assets/_MintTycoon/Audio/Feedback"

def write(name, duration, notes, noise=0.0):
    rng = random.Random(name)
    samples = []
    for i in range(int(duration * RATE)):
        time = i / RATE
        sample = 0.0
        for onset, frequency, length, gain in notes:
            local = time - onset
            if 0 <= local < length:
                envelope = min(1.0, local / 0.003) * math.exp(-local * 7 / length)
                sample += gain * envelope * (math.sin(2 * math.pi * frequency * local) + 0.15 * math.sin(2 * math.pi * frequency * 2.01 * local))
        sample += rng.uniform(-1, 1) * noise * math.exp(-time * 70)
        samples.append(struct.pack("<h", int(max(-1, min(1, sample)) * 32767)))
    OUTPUT.mkdir(parents=True, exist_ok=True)
    with wave.open(str(OUTPUT / (name + ".wav")), "wb") as sound:
        sound.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
        sound.writeframes(b"".join(samples))

write("OreTick", .085, [(0, 290, .08, .32)], noise=.16)
write("MintStrike", .13, [(0, 170, .12, .30), (0, 1320, .065, .13)], noise=.10)
write("CoinPickup", .12, [(0, 1046.5, .09, .22), (.025, 1568, .085, .10)])
write("OrderReward", .55, [(0, 523.25, .28, .24), (.09, 659.25, .29, .24), (.18, 783.99, .32, .24), (.27, 1046.5, .27, .19)])
write("ConveyorUnlock", .65, [(0, 392, .22, .20), (.08, 523.25, .25, .20), (.16, 659.25, .27, .20), (.25, 783.99, .35, .22)])
print("Generated five original mono feedback clips.")
