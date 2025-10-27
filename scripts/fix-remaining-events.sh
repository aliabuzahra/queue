#!/bin/bash

# More robust script to fix all domain events

# Function to fix a single event file
fix_event_file() {
    local file="$1"
    echo "Fixing $file..."
    
    if [[ -f "$file" ]]; then
        # Check if it already has the Id property
        if ! grep -q "public Guid Id" "$file"; then
            # Create a backup
            cp "$file" "$file.bak"
            
            # Use sed to replace the pattern
            sed -i 's/); : IDomainEvent;/) : IDomainEvent\n{\n    public Guid Id { get; } = Guid.NewGuid();\n    public DateTime OccurredOn { get; } = DateTime.UtcNow;\n}/' "$file"
            
            # Verify the change was made
            if grep -q "public Guid Id" "$file"; then
                echo "Successfully fixed $file"
                rm -f "$file.bak"
            else
                echo "Failed to fix $file, restoring backup"
                mv "$file.bak" "$file"
            fi
        else
            echo "Already fixed $file"
        fi
    else
        echo "File not found: $file"
    fi
}

# List of files that still need fixing (based on the error output)
FILES_TO_FIX=(
    "src/VirtualQueue.Domain/Events/QueueMergeOperationCreatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationFailedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationStartedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationProgressUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueMergeOperationCancelledEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateCreatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateDeactivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateActivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateUsedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserDeactivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/QueueTemplateVisibilityChangedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserActivatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserLoggedInEvent.cs"
    "src/VirtualQueue.Domain/Events/UserEmailUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserPhoneVerifiedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserPasswordUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserSuspendedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserProfileUpdatedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserTwoFactorEnabledEvent.cs"
    "src/VirtualQueue.Domain/Events/UserTwoFactorDisabledEvent.cs"
    "src/VirtualQueue.Domain/Events/UserEmailVerifiedEvent.cs"
    "src/VirtualQueue.Domain/Events/UserRoleUpdatedEvent.cs"
)

# Fix all files
for file in "${FILES_TO_FIX[@]}"; do
    fix_event_file "$file"
done

echo "All domain events have been fixed!"
