const fs = require('fs');
const path = require('path');
const GIFEncoder = require('gif-encoder-2');
const { PNG } = require('pngjs');

const directory = path.resolve(process.cwd(), '../../docs/screenshots');
const frameFiles = [
  '01-dashboard.png',
  '02-categories.png',
  '03-products.png',
  '04-customers.png',
  '05-orders.png'
];

const frames = frameFiles.map(file => {
  const fullPath = path.join(directory, file);
  if (!fs.existsSync(fullPath)) {
    throw new Error(`Screenshot não encontrado: ${fullPath}`);
  }
  return PNG.sync.read(fs.readFileSync(fullPath));
});

const { width, height } = frames[0];
for (const frame of frames) {
  if (frame.width !== width || frame.height !== height) {
    throw new Error('Todos os screenshots precisam ter o mesmo tamanho.');
  }
}

const output = path.join(directory, 'order-flow.gif');
const encoder = new GIFEncoder(width, height);
const writeStream = fs.createWriteStream(output);

encoder.createReadStream().pipe(writeStream);
encoder.start();
encoder.setRepeat(0);
encoder.setDelay(1400);
encoder.setQuality(10);

for (const frame of frames) {
  encoder.addFrame(frame.data);
}

encoder.finish();

writeStream.on('finish', () => {
  console.log(`GIF criado: ${output}`);
});
