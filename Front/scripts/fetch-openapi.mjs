// Downloads the backend's OpenAPI document into openapi/interface-api.json,
// which ng-openapi-gen then turns into the typed client in src/app/core/api/generated.
// Usage: npm run api:update   (the API must be running; override the URL with OPENAPI_URL)
import { mkdir, writeFile } from 'node:fs/promises';

const url = process.env.OPENAPI_URL ?? 'http://localhost:5019/swagger/v1/swagger.json';
const target = new URL('../openapi/interface-api.json', import.meta.url);

const response = await fetch(url);
if (!response.ok) {
  console.error(`Could not download ${url}: HTTP ${response.status}. Is the API running?`);
  process.exit(1);
}

const document = await response.json();
await mkdir(new URL('.', target), { recursive: true });
await writeFile(target, JSON.stringify(document, null, 2) + '\n');
console.log(`Saved OpenAPI document from ${url}`);
