#include <arm_neon.h>
#include <complex.h>
#include <math.h>

extern "C" {

void fft_radix4_neon(float* real, float* imag, int n, int inverse) {
    if (n % 4 != 0) return;

    for (int len = 4; len <= n; len <<= 2) {
        int half = len >> 1;
        int quarter = len >> 2;
        float angle = (inverse ? 2.0f : -2.0f) * M_PI / len;

        for (int i = 0; i < n; i += len) {
            for (int j = 0; j < quarter; j++) {
                int idx0 = i + j;
                int idx1 = i + j + quarter;
                int idx2 = i + j + half;
                int idx3 = i + j + half + quarter;

                float w1r = cosf(angle * j);
                float w1i = sinf(angle * j);
                float w2r = cosf(angle * 2 * j);
                float w2i = sinf(angle * 2 * j);
                float w3r = cosf(angle * 3 * j);
                float w3i = sinf(angle * 3 * j);

                float32x4_t ar = vld1q_f32(&real[idx0]);
                float32x4_t ai = vld1q_f32(&imag[idx0]);

                float32x4_t t0r = vaddq_f32(
                    vld1q_f32(&real[idx0]),
                    vld1q_f32(&real[idx2])
                );
                float32x4_t t0i = vaddq_f32(
                    vld1q_f32(&imag[idx0]),
                    vld1q_f32(&imag[idx2])
                );

                vst1q_f32(&real[idx0], t0r);
                vst1q_f32(&imag[idx0], t0i);
                vst1q_f32(&real[idx1], t0r);
                vst1q_f32(&imag[idx1], t0i);
            }
        }

        if (!inverse) {
            for (int i = 0; i < n; i++) {
                real[i] /= n;
                imag[i] /= n;
            }
        }
    }
}

}