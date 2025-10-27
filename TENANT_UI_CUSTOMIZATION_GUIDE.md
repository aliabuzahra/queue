# Tenant-Specific Queue UI Customization Guide

## Overview

Each tenant can have a completely customized waiting page UI with their own branding, colors, messaging, and behavior.

## Features

### 1. Per-Tenant UI Configuration

Each tenant can have:
- Custom branding (logo, name, tagline)
- Color theme (primary, secondary, gradients)
- Custom messages and CTAs
- Custom queue-specific messages
- Branded header with logo
- Tenant-specific metadata

## File Structure

```
src/VirtualQueue.Api/wwwroot/
├── index.html                    # Original demo page
├── waiting-page.html             # Standard waiting page
├── tenant-waiting-page.html      # Tenant-specific waiting page (NEW)
└── tenant-themes/                # Tenant-specific configurations
    ├── acme-corp.json
    ├── techstartup.json
    └── globalltd.json
```

## Implementation

### Configuration Options

Each tenant can configure their UI through a JSON configuration file stored in the database or file system.

#### Example: `acme-corp.json`

```json
{
  "tenant": {
    "id": "acme-corp-tenant-id",
    "name": "Acme Corporation",
    "domain": "acme.com",
    "branding": {
      "logo": "🚀",
      "logoImage": "/logos/acme-logo.svg",
      "title": "Welcome to Acme",
      "subtitle": "Your position is being processed",
      "footer": "© 2024 Acme Corporation"
    },
    "theme": {
      "primaryColor": "#FF6B35",
      "secondaryColor": "#F7931E",
      "backgroundColor": "#FFE6D9",
      "cardColor": "#FFFFFF",
      "textPrimary": "#1A1A1A",
      "textSecondary": "#666666"
    },
    "queueCustomizations": {
      "support": {
        "customMessage": "👥 Our support team is ready to help you!",
        "icon": "💬",
        "greeting": "Hello! We'll connect you with an agent soon."
      },
      "sales": {
        "customMessage": "💰 Special offer: 20% off all products today!",
        "icon": "🛍️",
        "greeting": "Welcome to our sales queue!"
      }
    },
    "features": {
      "showProgressBar": true,
      "showStatistics": true,
      "showEstimatedTime": true,
      "customCSS": ".tenant-logo { background: linear-gradient(45deg, #FF6B35, #F7931E); }",
      "customJavaScript": "console.log('Acme Corp customization loaded');"
    }
  }
}
```

## Usage

### Access URL

```
/tenant-waiting-page.html?tenantId={tenantId}&queueId={queueId}&userId={userId}
```

### Parameters

- `tenantId` (required): Tenant identifier
- `queueId` (required): Queue identifier
- `userId` (optional): User identifier (auto-generated if not provided)

### Example URLs

```
# Acme Corporation Support Queue
/tenant-waiting-page.html?tenantId=acme-tenant-123&queueId=support-queue-456

# Tech Startup Sales Queue
/tenant-waiting-page.html?tenantId=techstartup-789&queueId=sales-queue-012
```

## Dynamic Configuration Loading

The tenant-specific waiting page automatically:

1. **Loads Tenant Configuration**: Fetches tenant info from API
2. **Applies Theme**: Sets colors, fonts, and styling
3. **Displays Branding**: Shows tenant logo, name, and tagline
4. **Queue-Specific UI**: Shows custom messages for specific queues
5. **Real-Time Updates**: Connects to SignalR hub for live updates

## Customization Levels

### Level 1: Basic Branding

```json
{
  "branding": {
    "logo": "⏱️",
    "name": "Your Company Name",
    "tagline": "We're here to help"
  }
}
```

### Level 2: Custom Theme

```json
{
  "theme": {
    "primaryColor": "#1A73E8",
    "secondaryColor": "#0d47a1",
    "backgroundColor": "#E8F0FE"
  }
}
```

### Level 3: Queue-Specific Customization

```json
{
  "queueCustomizations": {
    "support": {
      "customMessage": "Our expert team is ready!",
      "icon": "💬"
    }
  }
}
```

### Level 4: Advanced Customization

```json
{
  "features": {
    "customCSS": "/* Custom CSS here */",
    "customJavaScript": "/* Custom JS here */"
  }
}
```

## API Integration

### Fetch Tenant Configuration

```javascript
GET /api/v1/tenants/{tenantId}

Response:
{
  "id": "tenant-id",
  "name": "Acme Corporation",
  "domain": "acme.com",
  "theme": { ... },
  "branding": { ... },
  "queueCustomizations": { ... }
}
```

### Fetch Queue Information

```javascript
GET /api/v1/tenants/{tenantId}/queues/{queueId}

Response:
{
  "id": "queue-id",
  "name": "Support Queue",
  "description": "...",
  "customMessage": "Welcome!",
  "maxConcurrentUsers": 10,
  ...
}
```

## UI Components

### Tenant Header
- Logo (emoji or image)
- Tenant name
- Tagline

### Position Card
- Large position number
- Estimated wait time
- Animated gradient

### Statistics Grid
- Users ahead
- Being served
- Completed

### Progress Bar
- Visual progress indicator
- Shimmer animation
- Real-time updates

### Custom Messages
- Queue-specific messages
- Important announcements
- Brand guidelines

## Responsive Design

The tenant-specific waiting page is fully responsive:

- **Desktop**: Full feature set with 600px max width
- **Tablet**: Adjusted spacing, same layout
- **Mobile**: Stacked layout, touch-friendly buttons
- **Dark Mode**: Automatic system preference detection

## Security & Privacy

- All tenant configurations are tenant-isolated
- No cross-tenant data leakage
- Secure API authentication required
- Privacy-compliant data handling

## Brand Guidelines

### Logo Usage
- Supports emoji (⏱️ 🚀 💼)
- Supports SVG images
- Supports PNG with transparency
- Recommended size: 60x60px

### Color Guidelines
- Use high contrast for accessibility
- Follow WCAG 2.1 AA standards
- Consider dark mode compatibility
- Brand colors should be consistent

### Message Guidelines
- Keep messages concise
- Use friendly, professional tone
- Consider multilingual support
- Update regularly for relevance

## Testing

### Test with Demo Mode

```url
/tenant-waiting-page.html?tenantId=demo&queueId=demo&demo=true
```

This will simulate queue updates with auto-decrementing position.

## Best Practices

1. **Brand Consistency**: Ensure UI matches your brand guidelines
2. **Loading Performance**: Keep custom CSS/JS minimal
3. **Mobile First**: Test on mobile devices
4. **Accessibility**: Maintain WCAG compliance
5. **User Experience**: Keep messages clear and concise
6. **Updates**: Provide real-time accurate information
7. **Error Handling**: Show friendly error messages

## Support

For customization help:
- Check the `queue-configuration-samples.json` file
- Review the base HTML structure
- Contact your implementation team

## Examples

### Technology Company (TechCorp)

```json
{
  "theme": {
    "primaryColor": "#0066FF",
    "backgroundColor": "#E6F3FF"
  },
  "branding": {
    "logo": "💻",
    "name": "TechCorp",
    "tagline": "Innovation Delivered"
  }
}
```

### Healthcare Provider (HealthFirst)

```json
{
  "theme": {
    "primaryColor": "#00A86B",
    "backgroundColor": "#E8F8F0"
  },
  "branding": {
    "logo": "🏥",
    "name": "HealthFirst",
    "tagline": "Your health, our priority"
  }
}
```

### E-commerce (ShopNow)

```json
{
  "theme": {
    "primaryColor": "#FF5722",
    "backgroundColor": "#FFF3E0"
  },
  "branding": {
    "logo": "🛒",
    "name": "ShopNow",
    "tagline": "Shop the world"
  }
}
```

## Future Enhancements

- Multi-language support
- Advanced animations
- Video background support
- Custom domain support
- A/B testing capabilities
- Analytics integration
