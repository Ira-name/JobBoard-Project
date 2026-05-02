import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.MY_URL || 'https://localhost:44344';
export const options = {
    vus: 1,
    duration: '1m',
    insecureSkipTLSVerify: true,
};

export default function () {
    const res = http.get(`${BASE_URL}/api/jobs`);

    check(res, {
        'status is 200': (r) => r.status === 200,
        'response time < 500ms': (r) => r.timings.duration < 500,
    });

    sleep(1);
}