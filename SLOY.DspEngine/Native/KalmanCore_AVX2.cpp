#include <immintrin.h>
#include <math.h>

extern "C" {

void kalman_filter_avx2(
    double* measurements, double* output,
    int count, double process_noise, double measurement_noise
) {
    __m256d q = _mm256_set1_pd(process_noise);
    __m256d r = _mm256_set1_pd(measurement_noise);
    __m256d x = _mm256_setzero_pd();
    __m256d p = _mm256_set1_pd(1.0);
    __m256d one = _mm256_set1_pd(1.0);

    for (int i = 0; i < count - 3; i += 4) {
        __m256d m = _mm256_loadu_pd(&measurements[i]);
        __m256d k, x_new;

        p = _mm256_add_pd(p, q);
        k = _mm256_div_pd(p, _mm256_add_pd(p, r));
        x_new = _mm256_add_pd(x, _mm256_mul_pd(k, _mm256_sub_pd(m, x)));
        p = _mm256_mul_pd(_mm256_sub_pd(one, k), p);

        _mm256_storeu_pd(&output[i], x_new);
        x = x_new;
    }

    for (int i = count - (count % 4); i < count; i++) {
        p.m256d_f64[0] += process_noise;
        double k = p.m256d_f64[0] / (p.m256d_f64[0] + measurement_noise);
        x.m256d_f64[0] += k * (measurements[i] - x.m256d_f64[0]);
        p.m256d_f64[0] *= (1 - k);
        output[i] = x.m256d_f64[0];
    }
}

}