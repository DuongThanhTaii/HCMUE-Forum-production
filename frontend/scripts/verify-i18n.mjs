import { readFileSync, readdirSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const projectRoot = path.resolve(__dirname, '..');

const viLocalesDir = path.join(projectRoot, 'src', 'shared', 'i18n', 'locales', 'vi');
const enLocalesDir = path.join(projectRoot, 'src', 'shared', 'i18n', 'locales', 'en');

const targetedFiles = [
  path.join(projectRoot, 'src', 'features', 'forum', 'components', 'ForumDetailPage.tsx'),
  path.join(projectRoot, 'src', 'features', 'forum', 'components', 'ForumListPage.tsx'),
  path.join(projectRoot, 'src', 'features', 'forum', 'components', 'ForumFiltersRow.tsx'),
  path.join(projectRoot, 'src', 'features', 'forum', 'components', 'ForumListTable.tsx'),
  path.join(projectRoot, 'src', 'app', 'router.tsx'),
  path.join(projectRoot, 'src', 'app', 'components', 'Placeholder.tsx'),
  path.join(projectRoot, 'src', 'shared', 'components', 'layouts', 'ForumTopbar.tsx'),
  path.join(projectRoot, 'src', 'shared', 'components', 'layouts', 'AuthLayout.tsx'),
];

function loadLocaleBundle(localesDir) {
  const bundle = {};
  const files = readdirSync(localesDir).filter((f) => f.endsWith('.json'));
  for (const file of files) {
    const ns = path.basename(file, '.json');
    const fullPath = path.join(localesDir, file);
    bundle[ns] = JSON.parse(readFileSync(fullPath, 'utf-8'));
  }
  return bundle;
}

function flattenKeys(source, prefix = '') {
  const result = [];
  for (const [key, value] of Object.entries(source)) {
    const fullKey = prefix ? `${prefix}.${key}` : key;
    result.push(fullKey);
    if (value && typeof value === 'object' && !Array.isArray(value)) {
      result.push(...flattenKeys(value, fullKey));
    }
  }
  return result;
}

function getKeysWithSpaces(keys) {
  return keys.filter((key) => key.split('.').some((segment) => segment.includes(' ')));
}

function checkDefaultValueDebt(files) {
  const offenders = [];
  for (const filePath of files) {
    const content = readFileSync(filePath, 'utf-8');
    if (content.includes('defaultValue')) {
      offenders.push(path.relative(projectRoot, filePath));
    }
  }
  return offenders;
}

const vi = loadLocaleBundle(viLocalesDir);
const en = loadLocaleBundle(enLocalesDir);
const viKeys = Array.from(new Set(flattenKeys(vi))).sort();
const enKeys = Array.from(new Set(flattenKeys(en))).sort();
const viSet = new Set(viKeys);
const enSet = new Set(enKeys);

const missingInEn = viKeys.filter((key) => !enSet.has(key));
const missingInVi = enKeys.filter((key) => !viSet.has(key));
const spacedKeys = [...new Set([...getKeysWithSpaces(viKeys), ...getKeysWithSpaces(enKeys)])].sort();
const defaultValueOffenders = checkDefaultValueDebt(targetedFiles);

const hasError =
  missingInEn.length > 0 ||
  missingInVi.length > 0 ||
  spacedKeys.length > 0 ||
  defaultValueOffenders.length > 0;

if (hasError) {
  console.error('i18n verification failed.');
  if (missingInEn.length > 0) {
    console.error('\nKeys missing in English locales (vs vi):');
    for (const key of missingInEn) {
      console.error(`- ${key}`);
    }
  }

  if (missingInVi.length > 0) {
    console.error('\nKeys missing in Vietnamese locales (vs en):');
    for (const key of missingInVi) {
      console.error(`- ${key}`);
    }
  }

  if (spacedKeys.length > 0) {
    console.error('\nKeys containing spaces (invalid schema):');
    for (const key of spacedKeys) {
      console.error(`- ${key}`);
    }
  }

  if (defaultValueOffenders.length > 0) {
    console.error('\nTargeted files still contain defaultValue fallback debt:');
    for (const file of defaultValueOffenders) {
      console.error(`- ${file}`);
    }
  }

  process.exit(1);
}

console.log('i18n verification passed.');
console.log(`- Locale parity keys: ${viKeys.length}`);
console.log('- No keys with spaces');
console.log('- No defaultValue debt in targeted files');
