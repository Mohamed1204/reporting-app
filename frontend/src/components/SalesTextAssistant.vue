<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import {
  extractSalesEntry,
  MAX_EXTRACTION_TEXT_LENGTH,
  type SalesEntryDraft,
} from '../services/salesExtraction'

const props = defineProps<{
  startDate: string
  endDate: string
  hasFormValues: boolean
  countryOptions: Array<{ label: string; value: string }>
  disabled?: boolean
}>()
const emit = defineEmits<{ apply: [draft: SalesEntryDraft] }>()

const sourceText = ref('')
const draft = ref<SalesEntryDraft | null>(null)
const isExtracting = ref(false)
const error = ref('')
const notice = ref('')
const confirmEuro = ref(false)
const confirmReplace = ref(false)
let activeRequest: AbortController | null = null

const canExtract = computed(
  () =>
    !props.disabled &&
    !isExtracting.value &&
    sourceText.value.trim().length > 0 &&
    sourceText.value.length <= MAX_EXTRACTION_TEXT_LENGTH,
)
const unsupportedCurrency = computed(() => draft.value?.currency && draft.value.currency !== 'EUR')
const unsupportedCountry = computed(
  () =>
    draft.value?.buyerCountry &&
    !props.countryOptions.some((c) => c.value === draft.value?.buyerCountry),
)
const outsidePeriod = computed(
  () =>
    !!draft.value?.saleDate &&
    (draft.value.saleDate < props.startDate || draft.value.saleDate > props.endDate),
)
const canApply = computed(
  () =>
    !!draft.value &&
    !props.disabled &&
    !unsupportedCurrency.value &&
    !unsupportedCountry.value &&
    (draft.value.currency === 'EUR' || confirmEuro.value) &&
    (!props.hasFormValues || confirmReplace.value),
)
const preview = computed(() => {
  const value = draft.value
  if (!value) return []
  return [
    {
      label: 'Country',
      value:
        props.countryOptions.find((c) => c.value === value.buyerCountry)?.label ??
        value.buyerCountry,
    },
    {
      label: 'Amount before VAT',
      value:
        value.amount === null
          ? null
          : new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value.amount),
    },
    { label: 'Currency', value: value.currency },
    { label: 'Buyer type', value: value.buyerType },
    { label: 'Product category', value: value.productCategory },
    { label: 'Sale date', value: value.saleDate },
  ]
})

function cancelRequest() {
  activeRequest?.abort()
  activeRequest = null
  isExtracting.value = false
}

function clearPreview() {
  cancelRequest()
  draft.value = null
  error.value = ''
  notice.value = ''
  confirmEuro.value = false
  confirmReplace.value = false
}

watch(sourceText, clearPreview)
watch(
  () => props.disabled,
  (disabled) => {
    if (disabled) clearPreview()
  },
)
onBeforeUnmount(cancelRequest)

function useExample() {
  const date = props.startDate || '2025-07-01'
  sourceText.value = `Sold books to a consumer in Germany on ${date}. The amount before VAT was EUR 200.`
}

async function getSuggestions() {
  if (!canExtract.value) return
  clearPreview()
  const controller = new AbortController()
  activeRequest = controller
  isExtracting.value = true
  const timeout = setTimeout(() => controller.abort(), 30_000)
  try {
    const result = await extractSalesEntry(sourceText.value.trim(), controller.signal)
    if (activeRequest === controller && !controller.signal.aborted) draft.value = result
  } catch (err) {
    if (activeRequest !== controller) return
    error.value = controller.signal.aborted
      ? 'This is taking too long. Try again or enter the sale manually.'
      : err instanceof TypeError
        ? 'Could not connect to the suggestion service. Please try again.'
        : err instanceof Error
          ? err.message
          : 'Could not generate suggestions. Please try again.'
  } finally {
    clearTimeout(timeout)
    if (activeRequest === controller) {
      activeRequest = null
      isExtracting.value = false
    }
  }
}

function applySuggestions() {
  if (!canApply.value || !draft.value) return
  emit('apply', { ...draft.value, currency: 'EUR' })
  draft.value = null
  notice.value =
    'Suggestions copied to the form. Review the values and complete any empty fields before adding the sale.'
}
</script>

<template>
  <section class="text-assistant" aria-labelledby="text-assistant-title">
    <div class="assistant-heading">
      <div>
        <p class="eyebrow">AI assistance</p>
        <h3 id="text-assistant-title">Fill from text</h3>
      </div>
      <span class="review-badge">You review before saving</span>
    </div>
    <p class="intro">
      Describe one sale or paste invoice text. Get suggested values for the form below.
    </p>

    <form @submit.prevent="getSuggestions">
      <label for="sales-source-text">Sales or invoice text</label>
      <textarea
        id="sales-source-text"
        v-model="sourceText"
        name="salesSourceText"
        rows="4"
        :maxlength="MAX_EXTRACTION_TEXT_LENGTH"
        :disabled="disabled"
        aria-describedby="sales-text-help sales-text-count"
        placeholder="For example: Sold books to a consumer in Germany. Net amount EUR 200. Include the sale date."
      />
      <div class="input-help">
        <span id="sales-text-help"
          >Include the country, amount before VAT, buyer type, category, and date when known.</span
        >
        <span id="sales-text-count"
          >{{ sourceText.length.toLocaleString() }} /
          {{ MAX_EXTRACTION_TEXT_LENGTH.toLocaleString() }}</span
        >
      </div>
      <div class="assistant-actions">
        <button type="submit" class="primary" :disabled="!canExtract">
          {{ isExtracting ? 'Reading your text...' : 'Suggest form values' }}
        </button>
        <button v-if="isExtracting" type="button" class="secondary" @click="cancelRequest">
          Cancel
        </button>
        <button
          v-else
          type="button"
          class="secondary"
          :disabled="disabled || !!sourceText"
          @click="useExample"
        >
          Use example text
        </button>
      </div>
    </form>

    <p v-if="isExtracting" role="status" class="feedback">
      Finding the sales details. You can cancel or edit your text.
    </p>
    <p v-if="error" role="alert" class="feedback error">{{ error }}</p>
    <p v-if="notice" role="status" class="feedback success">{{ notice }}</p>

    <section
      v-if="draft"
      class="suggestion-preview"
      aria-labelledby="suggestions-title"
      aria-live="polite"
    >
      <h4 id="suggestions-title">Review suggestions</h4>
      <p>Check these against your text. Missing information will stay blank in the form.</p>
      <dl>
        <div v-for="field in preview" :key="field.label">
          <dt>{{ field.label }}</dt>
          <dd :class="{ missing: field.value === null }">{{ field.value ?? 'Not found' }}</dd>
        </div>
      </dl>
      <p v-if="unsupportedCurrency" class="feedback error">
        This report accepts EUR only. The text uses {{ draft.currency }}; no currency conversion
        will be applied.
      </p>
      <p v-if="unsupportedCountry" class="feedback error">
        The suggested country is not supported by this form. Check your text or enter the sale
        manually.
      </p>
      <p v-if="outsidePeriod" class="feedback warning">
        The suggested date is outside this reporting period. Correct it in the form before adding
        the sale.
      </p>
      <p v-if="draft.buyerType === 'B2B'" class="feedback warning">
        Check the buyer's VAT-number status yourself. These suggestions do not verify it.
      </p>
      <label v-if="!draft.currency" class="confirmation">
        <input v-model="confirmEuro" type="checkbox" name="confirmEuro" />
        I confirm the amount is in EUR.
      </label>
      <label v-if="hasFormValues" class="confirmation">
        <input v-model="confirmReplace" type="checkbox" name="confirmReplace" />
        Replace my current unsaved form values with these suggestions. Missing values will be
        cleared.
      </label>
      <div class="assistant-actions">
        <button type="button" class="primary" :disabled="!canApply" @click="applySuggestions">
          Use suggestions
        </button>
        <button type="button" class="secondary" @click="clearPreview">Discard suggestions</button>
      </div>
      <p class="footnote">
        This fills the form only. Add the entry and save your draft when you are ready.
      </p>
    </section>
  </section>
</template>

<style scoped>
.text-assistant {
  margin: 1rem 0 1.25rem;
  padding: 1.2rem;
  border: 1px solid #bbd9cc;
  border-radius: 12px;
  background: #f5fbf8;
  color: #203342;
}
.assistant-heading,
.input-help,
.assistant-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}
.eyebrow {
  margin: 0 0 0.25rem;
  color: #13795b;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}
h3,
h4 {
  margin: 0;
  color: #12202b;
}
h3 {
  font-size: 1.2rem;
}
.review-badge {
  padding: 0.3rem 0.6rem;
  border-radius: 99px;
  background: #e2f2e9;
  color: #205b43;
  font-size: 0.8rem;
}
.intro,
.suggestion-preview > p {
  margin: 0.5rem 0 0.9rem;
  color: #51616f;
}
form > label {
  display: block;
  margin-bottom: 0.4rem;
  font-weight: 600;
}
textarea {
  display: block;
  box-sizing: border-box;
  width: 100%;
  resize: vertical;
  min-height: 7rem;
  padding: 0.75rem;
  border: 1px solid #aebfb7;
  border-radius: 8px;
  background: white;
  color: #203342;
  font: inherit;
  line-height: 1.5;
}
textarea:focus-visible,
button:focus-visible,
input:focus-visible {
  outline: 3px solid #82baaa;
  outline-offset: 3px;
}
.input-help {
  align-items: start;
  margin: 0.45rem 0 0.9rem;
  font-size: 0.8rem;
  color: #51616f;
}
.input-help span:last-child {
  white-space: nowrap;
}
.assistant-actions {
  justify-content: flex-start;
}
button {
  border: 1px solid transparent;
  border-radius: 8px;
  padding: 0.6rem 0.85rem;
  cursor: pointer;
  font: inherit;
  font-weight: 600;
}
button.primary {
  background: #13795b;
  color: white;
}
button.primary:hover:enabled {
  background: #0f654b;
}
button.secondary {
  border-color: #c7d8d0;
  background: white;
  color: #285241;
}
button:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.feedback {
  margin: 0.85rem 0 0;
}
.feedback.error {
  color: #a32929;
}
.feedback.warning {
  color: #795215;
}
.feedback.success {
  color: #156e52;
}
.suggestion-preview {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #c7d8d0;
}
dl {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 0.75rem;
  margin: 0.85rem 0;
}
dl > div {
  padding: 0.7rem;
  background: white;
  border: 1px solid #dce8e1;
  border-radius: 8px;
  overflow-wrap: anywhere;
}
dt {
  color: #51616f;
  font-size: 0.8rem;
}
dd {
  margin: 0.3rem 0 0;
  font-weight: 600;
}
dd.missing {
  color: #795215;
  font-weight: 400;
}
.confirmation {
  display: flex;
  align-items: start;
  gap: 0.6rem;
  margin: 0.85rem 0;
}
.confirmation input {
  margin-top: 0.25rem;
}
.suggestion-preview .assistant-actions {
  margin-top: 0.9rem;
}
.suggestion-preview .footnote {
  margin: 0.75rem 0 0;
  font-size: 0.85rem;
}
@media (max-width: 480px) {
  .text-assistant {
    padding: 0.85rem;
  }
  .assistant-actions button {
    width: 100%;
  }
}
</style>
