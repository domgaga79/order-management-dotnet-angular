import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  testMatch: /portfolio\.capture\.spec\.ts/,
  timeout: 60_000,
  use: {
    baseURL: process.env.PORTFOLIO_BASE_URL ?? 'http://localhost:4200',
    viewport: { width: 1200, height: 750 },
    ...devices['Desktop Chrome']
  }
});
