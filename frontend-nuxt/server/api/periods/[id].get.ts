export default defineEventHandler((event) => {
  console.log('ran [id] handler')

  const id = Number(getRouterParam(event, 'id'))

  if (!Number.isInteger(id)) {
    throw createError({ statusCode: 400, statusMessage: 'Period id must be an integer' })
  }

  const period = periods.find((p) => p.id === id)

  if (!period) {
    throw createError({ statusCode: 404, statusMessage: 'Period not found' })
  }

  return period
})

//easy wins Lille H  Sabah H  Slavia A
// Medium Betis A  Dortmund H
// Tuff  R.Madrid  Bayern  Napoli A