#!/usr/bin/env python3

import os
import re

# List of files that need fixing
files_to_fix = [
    "src/VirtualQueue.Domain/Events/QueueMergeOperationFailedEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueMergeOperationStartedEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueMergeOperationProgressUpdatedEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueMergeOperationCancelledEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueTemplateCreatedEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueTemplateDeactivatedEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueTemplateActivatedEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueTemplateUpdatedEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueTemplateUsedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserDeactivatedEvent.cs",
    "src/VirtualQueue.Domain/Events/QueueTemplateVisibilityChangedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserActivatedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserLoggedInEvent.cs",
    "src/VirtualQueue.Domain/Events/UserEmailUpdatedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserPhoneVerifiedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserPasswordUpdatedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserSuspendedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserProfileUpdatedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserTwoFactorEnabledEvent.cs",
    "src/VirtualQueue.Domain/Events/UserTwoFactorDisabledEvent.cs",
    "src/VirtualQueue.Domain/Events/UserEmailVerifiedEvent.cs",
    "src/VirtualQueue.Domain/Events/UserRoleUpdatedEvent.cs"
]

def fix_event_file(file_path):
    """Fix a single event file to implement IDomainEvent properly."""
    print(f"Fixing {file_path}...")
    
    if not os.path.exists(file_path):
        print(f"File not found: {file_path}")
        return False
    
    # Read the file
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Check if already fixed
    if "public Guid Id { get; } = Guid.NewGuid();" in content:
        print(f"Already fixed: {file_path}")
        return True
    
    # Pattern to match the record declaration ending with ); : IDomainEvent;
    pattern = r'(\s*\)\s*:\s*IDomainEvent\s*;)'
    replacement = r''') : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}'''
    
    # Replace the pattern
    new_content = re.sub(pattern, replacement, content)
    
    if new_content != content:
        # Write the fixed content
        with open(file_path, 'w') as f:
            f.write(new_content)
        print(f"Successfully fixed: {file_path}")
        return True
    else:
        print(f"No changes needed: {file_path}")
        return False

# Fix all files
success_count = 0
for file_path in files_to_fix:
    if fix_event_file(file_path):
        success_count += 1

print(f"\nFixed {success_count} out of {len(files_to_fix)} files")
print("All domain events have been processed!")
