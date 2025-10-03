/**
 * Form validation helpers with i18n support
 */

export interface ValidationResult {
  valid: boolean
  message?: string
}

export class FormValidators {
  /**
   * Required field validator
   */
  static required(value: any, message?: string): boolean | string {
    if (value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0)) {
      return message || 'validation.required'
    }
    return true
  }

  /**
   * Phone number validator (Chinese mobile number)
   */
  static phone(value: string, message?: string): boolean | string {
    if (!value) return true // Use with required() for mandatory fields
    const phoneRegex = /^1[3-9]\d{9}$/
    return phoneRegex.test(value) || (message || 'validation.phoneInvalid')
  }

  /**
   * Email validator
   */
  static email(value: string, message?: string): boolean | string {
    if (!value) return true
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(value) || (message || 'validation.emailInvalid')
  }

  /**
   * Minimum length validator
   */
  static minLength(min: number, message?: string) {
    return (value: string): boolean | string => {
      if (!value) return true
      return value.length >= min || (message || `validation.minLength`)
    }
  }

  /**
   * Maximum length validator
   */
  static maxLength(max: number, message?: string) {
    return (value: string): boolean | string => {
      if (!value) return true
      return value.length <= max || (message || `validation.maxLength`)
    }
  }

  /**
   * Minimum value validator
   */
  static minValue(min: number, message?: string) {
    return (value: number): boolean | string => {
      if (value === null || value === undefined) return true
      return value >= min || (message || `validation.minValue`)
    }
  }

  /**
   * Maximum value validator
   */
  static maxValue(max: number, message?: string) {
    return (value: number): boolean | string => {
      if (value === null || value === undefined) return true
      return value <= max || (message || `validation.maxValue`)
    }
  }

  /**
   * Password validator (minimum length)
   */
  static password(minLength: number = 6, message?: string) {
    return (value: string): boolean | string => {
      if (!value) return true
      return value.length >= minLength || (message || `validation.passwordMinLength`)
    }
  }

  /**
   * Password match validator
   */
  static passwordMatch(password: string, message?: string) {
    return (value: string): boolean | string => {
      return value === password || (message || 'validation.passwordMismatch')
    }
  }

  /**
   * Date validator - not in the past
   */
  static futureDate(message?: string) {
    return (value: Date | string): boolean | string => {
      if (!value) return true
      const date = typeof value === 'string' ? new Date(value) : value
      const today = new Date()
      today.setHours(0, 0, 0, 0)
      return date >= today || (message || 'validation.pastDate')
    }
  }

  /**
   * Date validator - not in the future
   */
  static pastDate(message?: string) {
    return (value: Date | string): boolean | string => {
      if (!value) return true
      const date = typeof value === 'string' ? new Date(value) : value
      const today = new Date()
      today.setHours(23, 59, 59, 999)
      return date <= today || (message || 'validation.futureDate')
    }
  }

  /**
   * URL validator
   */
  static url(message?: string) {
    return (value: string): boolean | string => {
      if (!value) return true
      try {
        new URL(value)
        return true
      } catch {
        return message || 'validation.invalidUrl'
      }
    }
  }

  /**
   * Pattern validator
   */
  static pattern(regex: RegExp, message?: string) {
    return (value: string): boolean | string => {
      if (!value) return true
      return regex.test(value) || (message || 'validation.invalidFormat')
    }
  }

  /**
   * Custom validator
   */
  static custom(validator: (value: any) => boolean, message: string) {
    return (value: any): boolean | string => {
      return validator(value) || message
    }
  }

  /**
   * Combine multiple validators
   */
  static combine(...validators: Array<(value: any) => boolean | string>) {
    return (value: any): boolean | string => {
      for (const validator of validators) {
        const result = validator(value)
        if (result !== true) {
          return result
        }
      }
      return true
    }
  }
}

// Export commonly used validators as standalone functions
export const {
  required,
  phone,
  email,
  minLength,
  maxLength,
  minValue,
  maxValue,
  password,
  passwordMatch,
  futureDate,
  pastDate,
  url,
  pattern,
  custom,
  combine,
} = FormValidators

