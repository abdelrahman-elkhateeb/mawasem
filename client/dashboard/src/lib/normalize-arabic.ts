const ARABIC_DIACRITICS_REGEX = /[\u064B-\u0652\u0670\u06D6-\u06ED]/g;
const TATWEEL_REGEX = /\u0640/g;

export function normalizeArabic(value: string) {
  return value
    .replace(ARABIC_DIACRITICS_REGEX, "")
    .replace(TATWEEL_REGEX, "")
    .replace(/[أإآ]/g, "ا")
    .replace(/ى/g, "ي")
    .replace(/ة/g, "ه")
    .trim()
    .toLowerCase();
}