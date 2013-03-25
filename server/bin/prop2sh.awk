/^#/ { next }
/^!/ { next }
/^[:space:]*$/ { next }
{
    sub(/^[ \t]+/, "", $0)
    equal = match($0,/[=:]/)
    if (equal == 0) {
	key = $0
	sub(/^[ \t]+/, "", key)
	sub(/[ \t]+$/, "", key)
	printf "%s=''\n", key
	next
    }
    key = substr($0,1,equal-1)
    value = substr($0,equal+1)
    sub(/^[ \t]+/, "", key)
    sub(/[ \t]+$/, "", key)
    gsub(/[.]/, "_", key)
    sub(/^[ \t]+/, "", value)
    sub(/[ \t]+$/, "", value)

    gsub(/[\\][\\]/, "\\", value)
    #print
    #print "KEY:", key, "."
    #print "VALUE:", value, "."
    printf "%s='%s'\n", key, value
}
