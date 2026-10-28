import './LoginRegisterView.css'

// Port of frontend/src/views/LoginRegisterView.vue
// Markup only: state, handlers and the login/register switch are yours to add.
export default function LoginRegisterView() {
  return (
    <div className="auth-container">
      <div>
        <h2>Login</h2>
        <form>
          <input name="UserName" placeholder="Username" required />
          <input name="Password" type="password" placeholder="Password" required />
          <button type="submit">Login</button>
        </form>
        <p>
          Don't have an account? <a href="#">Register</a>
        </p>
      </div>

      <div>
        <h2>Register</h2>
        <form>
          <input name="UserName" placeholder="Username" required />
          <input name="Password" type="password" placeholder="Password" required />
          <select name="companyId" defaultValue="" required>
            <option value="" disabled>
              Select Company
            </option>
            {/* company options go here */}
          </select>
          <button type="submit">Register</button>
        </form>
        <p>
          Already have an account? <a href="#">Login</a>
        </p>
      </div>

      <div className="error">{/* error message goes here */}</div>
    </div>
  )
}
