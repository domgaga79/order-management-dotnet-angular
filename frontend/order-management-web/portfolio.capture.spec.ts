import { expect, test } from '@playwright/test';
import fs from 'node:fs/promises';
import path from 'node:path';

const outputDir = path.resolve(process.cwd(), '../../docs/screenshots');

test('capture portfolio screenshots', async ({ page }) => {
  await fs.mkdir(outputDir, { recursive: true });

  await page.goto('/login');
  await page.getByLabel('E-mail').fill('admin@local.test');
  await page.getByLabel('Senha').fill('Admin123!');
  await page.getByRole('button', { name: 'Entrar' }).click();
  await page.waitForURL(url => !url.pathname.includes('/login'));

  const pages = [
    { route: '/', file: '01-dashboard.png' },
    { route: '/categories', file: '02-categories.png' },
    { route: '/products', file: '03-products.png' },
    { route: '/customers', file: '04-customers.png' },
    { route: '/orders', file: '05-orders.png' }
  ];

  for (const item of pages) {
    await page.goto(item.route);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toBeVisible();
    await page.screenshot({
      path: path.join(outputDir, item.file),
      fullPage: false
    });
  }
});
