import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.MY_URL || 'https://localhost:44344';
export const options = {
    insecureSkipTLSVerify: true,
    thresholds: {
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.01'],
    },
    
    scenarios: {
        stress_test: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '2m', target: 10 },
                { duration: '2m', target: 50 },
                { duration: '2m', target: 100 },
            ],
        },
    },
};

export function setup() {
    const res = http.get(`${BASE_URL}/api/jobs`);
    const jobs = res.json();

    if (!jobs || jobs.length === 0) {
        throw new Error("Немає вакансій для тесту");
    }

    console.log(`Завантажено ${jobs.length} вакансій`);

    return { jobs };
}

export default function (data) {

    const job = data.jobs[Math.floor(Math.random() * data.jobs.length)];

    const payload = JSON.stringify({
        applicantName: 'Test User',
        email: `test_${__VU}_${__ITER}_${Date.now()}@example.com`,
        resumeUrl: 'https://example.com/resume.pdf',
        coverLetter: 'This is my professional cover letter for this position.'
    });

    const res = http.post(
        `${BASE_URL}/api/jobs/${job.id}/apply`,
        payload,
        { headers: { 'Content-Type': 'application/json' } }
    );

    check(res, {
        'status is 200': (r) => {
            if (r.status !== 200) {
                console.log(` ${r.status} - ${r.body}`);
            }
            return r.status === 200;
        }
    });

    sleep(1);
}