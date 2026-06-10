describe('login flow', () => {
  // The home page fetches VAT reports on mount. Stub it so the landing
  // page renders deterministically without a real backend.
  const stubReports = () =>
    cy.intercept('GET', '**/api/VatReports*', {
      statusCode: 200,
      body: { items: [], totalCount: 0, page: 1, pageSize: 50 },
    })

  it('logs in with valid credentials and lands on the home page', () => {
    cy.intercept('POST', '**/api/Auth/login', {
      statusCode: 200,
      body: { token: 'fake-jwt', role: 'User', userName: 'alice', companyName: 'Acme ApS' },
    }).as('login')
    stubReports()

    cy.visit('/auth')

    cy.get('input[placeholder="Username"]').clear().type('alice')
    cy.get('input[placeholder="Password"]').clear().type('s3cret')
    cy.contains('button', 'Login').click()

    cy.wait('@login')
    cy.location('pathname').should('eq', '/')
    cy.contains('h1', 'Open Periods').should('be.visible')
  })

  it('shows an error message on invalid credentials', () => {
    cy.intercept('POST', '**/api/Auth/login', { statusCode: 401, body: {} }).as('login')

    cy.visit('/auth')

    cy.get('input[placeholder="Username"]').clear().type('alice')
    cy.get('input[placeholder="Password"]').clear().type('wrong')
    cy.contains('button', 'Login').click()

    cy.wait('@login')
    cy.contains('.error', 'Invalid username or password.').should('be.visible')
    cy.location('pathname').should('eq', '/auth')
  })
})
