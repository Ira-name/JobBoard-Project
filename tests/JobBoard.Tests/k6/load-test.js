import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    insecureSkipTLSVerify: true,
    stages: [
        { duration: '30s', target: 50 }, // розгін
        { duration: '4m', target: 10 },  // стабільне навантаження
        { duration: '30s', target: 0 },  // спад
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.01'],
    },
};

export default function () {
    const url = 'https://localhost:44344/api/jobs?location=remote';

    const res = http.get(url);

    check(res, {
        'status is 200': (r) => r.status === 200,
        'fast response': (r) => r.timings.duration < 500,
    });

    sleep(1);
    
}